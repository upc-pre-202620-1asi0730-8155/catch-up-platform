using CatchUpPlatform.API.News.Application.Errors;
using CatchUpPlatform.API.News.Domain.Model.Aggregates;
using CatchUpPlatform.API.Resources;
using CatchUpPlatform.API.Shared.Application.Patterns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CatchUpPlatform.API.News.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of the command handling to create a favorite source in the application layer.
/// </summary>
public static class ActionResultFromCreateFavoriteSourceResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult" /> from the result of a create favorite source operation.
    /// </summary>
    /// <param name="result">The application result produced by the application layer after handling the command to create a favorite source.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <param name="localizer">The string localizer used for API response messages.</param>
    /// <param name="getFavoriteSourceByIdActionName">The name of the action used to generate the location header.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromCreateFavoriteSourceResult(
        Result<FavoriteSource, ECreateFavoriteSourceError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer,
        string getFavoriteSourceByIdActionName) =>
        result switch
        {
            Result<FavoriteSource, ECreateFavoriteSourceError>.Success success =>
                controller.CreatedAtAction(getFavoriteSourceByIdActionName, new { id = success.Value.Id },
                    FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),

            Result<FavoriteSource, ECreateFavoriteSourceError>.Failure failure =>
                failure.Error switch
                {
                    ECreateFavoriteSourceError.DuplicateFavoriteSource =>
                        controller.Conflict(localizer["NewsFavoriteSourceDuplicated"].Value),

                    ECreateFavoriteSourceError.UnexpectedError =>
                        controller.Problem(
                            title: localizer["UnexpectedServerError"].Value,
                            detail: localizer["UnexpectedErrorCreatingFavoriteSource"].Value,
                            statusCode: 500),

                    _ => controller.Problem(
                        title: localizer["UnexpectedServerError"].Value,
                        detail: localizer["UnexpectedErrorProcessingRequest"].Value,
                        statusCode: 500)
                },

            _ => controller.Problem(
                title: localizer["UnexpectedServerError"].Value,
                detail: localizer["UnexpectedErrorProcessingRequest"].Value,
                statusCode: 500)
        };
}



