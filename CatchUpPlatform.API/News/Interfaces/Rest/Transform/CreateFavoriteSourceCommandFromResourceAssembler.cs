using CatchUpPlatform.API.News.Domain.Model.Commands;
using CatchUpPlatform.API.News.Domain.Model.ValueObjects;
using CatchUpPlatform.API.News.Interfaces.Rest.Resources;

namespace CatchUpPlatform.API.News.Interfaces.Rest.Transform;

/// <summary>
///     Assembles a CreateFavoriteSourceCommand from a CreateFavoriteSourceResource.
/// </summary>
public static class CreateFavoriteSourceCommandFromResourceAssembler
{
    /// <summary>
    ///     Assembles a CreateFavoriteSourceCommand from a CreateFavoriteSourceResource.
    /// </summary>
    /// <param name="resource">The CreateFavoriteSourceResource resource</param>
    /// <returns>
    ///     A CreateFavoriteSourceCommand assembled from the CreateFavoriteSourceResource
    /// </returns>
    public static CreateFavoriteSourceCommand ToCommandFromResource(CreateFavoriteSourceResource resource) =>
        new(new NewsApiKey(resource.NewsApiKey), new SourceId(resource.SourceId));
}