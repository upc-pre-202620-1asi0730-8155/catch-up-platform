using CatchUpPlatform.API.News.Domain.Model.Aggregates;
using CatchUpPlatform.API.News.Application.Errors;
using CatchUpPlatform.API.News.Application.Services;
using CatchUpPlatform.API.News.Domain.Model.Commands;
using CatchUpPlatform.API.News.Domain.Repositories;
using CatchUpPlatform.API.Shared.Domain.Repositories;
using CatchUpPlatform.API.Shared.Application.Patterns;
using Microsoft.EntityFrameworkCore;

namespace CatchUpPlatform.API.News.Application.Internal.CommandServices;

/// <summary>
///     Application service for handling favorite source creation commands.
/// </summary>
/// <remarks>
///     This service acts as the command handler for creating new favorite sources.
///     It enforces duplicate detection through both application-level checks and database constraints,
///     then coordinates with the unit of work to persist changes.
///     Logs warnings for duplicate detections and errors for persistence failures.
/// </remarks>
/// <param name="favoriteSourceRepository">Repository for accessing favorite source data.</param>
/// <param name="unitOfWork">Unit of work for managing transaction scope.</param>
/// <param name="logger">Logger for diagnostic and error reporting.</param>
/// See
/// <see cref="IFavoriteSourceRepository">IFavoriteSourceRepository</see>
/// ,
/// <see cref="IUnitOfWork">IUnitOfWork</see>
public class FavoriteSourceCommandService(
    IFavoriteSourceRepository favoriteSourceRepository,
    IUnitOfWork unitOfWork,
    ILogger<FavoriteSourceCommandService> logger)
    : IFavoriteSourceCommandService
{
    /// <inheritdoc />
    public async Task<Result<FavoriteSource, ECreateFavoriteSourceError>> Handle(CreateFavoriteSourceCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingSource =
            await favoriteSourceRepository.FindByNewsApiKeyAndSourceIdAsync(command.NewsApiKey, command.SourceId,
                cancellationToken);
        if (existingSource != null)
        {
            logger.LogWarning(
                "Duplicate favorite source rejected for NewsApiKey {NewsApiKey} and SourceId {SourceId}",
                command.NewsApiKey.MaskedForLogging(),
                command.SourceId);
            return new Result<FavoriteSource, ECreateFavoriteSourceError>.Failure(
                ECreateFavoriteSourceError.DuplicateFavoriteSource);
        }

        try
        {
            var favoriteSource = new FavoriteSource(command);
            await favoriteSourceRepository.AddAsync(favoriteSource, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<FavoriteSource, ECreateFavoriteSourceError>.Success(favoriteSource);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex,
                "Invalid arguments while creating favorite source for NewsApiKey {NewsApiKey} and SourceId {SourceId}",
                command.NewsApiKey.MaskedForLogging(),
                command.SourceId);
            return new Result<FavoriteSource, ECreateFavoriteSourceError>.Failure(
                ECreateFavoriteSourceError.UnexpectedError);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            logger.LogWarning(ex,
                "Duplicate key violation creating favorite source for NewsApiKey {NewsApiKey} and SourceId {SourceId}",
                command.NewsApiKey.MaskedForLogging(),
                command.SourceId);
            return new Result<FavoriteSource, ECreateFavoriteSourceError>.Failure(
                ECreateFavoriteSourceError.DuplicateFavoriteSource);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex,
                "Database update failed creating favorite source for NewsApiKey {NewsApiKey} and SourceId {SourceId}",
                command.NewsApiKey.MaskedForLogging(),
                command.SourceId);
            return new Result<FavoriteSource, ECreateFavoriteSourceError>.Failure(
                ECreateFavoriteSourceError.UnexpectedError);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error creating favorite source for NewsApiKey {NewsApiKey} and SourceId {SourceId}",
                command.NewsApiKey.MaskedForLogging(),
                command.SourceId);
            return new Result<FavoriteSource, ECreateFavoriteSourceError>.Failure(
                ECreateFavoriteSourceError.UnexpectedError);
        }
    }

    /// <summary>
    /// Determines whether a DbUpdateException represents a duplicate key constraint violation.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns>True if the exception is due to a MySQL duplicate key error (code 1062), false otherwise.</returns>
    private static bool IsDuplicateKeyViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (!string.Equals(current.GetType().Name, "MySqlException", StringComparison.Ordinal)) continue;
            var numberProperty = current.GetType().GetProperty("Number");
            if (numberProperty?.PropertyType == typeof(int) &&
                numberProperty.GetValue(current) is int errorCode &&
                errorCode == 1062)
                return true;
        }
        return false;
    }
}