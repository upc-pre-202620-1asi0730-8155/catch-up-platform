using CatchUpPlatform.API.News.Application.Errors;
using CatchUpPlatform.API.News.Domain.Model.Aggregates;
using CatchUpPlatform.API.News.Domain.Model.Commands;
using CatchUpPlatform.API.Shared.Application.Patterns;

namespace CatchUpPlatform.API.News.Application.Services;

public interface IFavoriteSourceCommandService
{
    Task<Result<FavoriteSource, ECreateFavoriteSourceError>> Handle(
        CreateFavoriteSourceCommand command,
        CancellationToken cancellationToken = default);
}