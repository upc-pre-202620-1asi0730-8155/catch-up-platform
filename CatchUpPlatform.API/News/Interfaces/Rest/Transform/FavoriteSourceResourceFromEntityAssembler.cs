using CatchUpPlatform.API.News.Domain.Model.Aggregates;
using CatchUpPlatform.API.News.Interfaces.Rest.Resources;

namespace CatchUpPlatform.API.News.Interfaces.Rest.Transform;

/// <summary>
///     Assembles a FavoriteSourceResource from a FavoriteSource.
/// </summary>
public static class FavoriteSourceResourceFromEntityAssembler
{
    /// <summary>
    ///     Assembles a FavoriteSourceResource from a FavoriteSource.
    /// </summary>
    /// <param name="entity">The FavoriteSource entity</param>
    /// <returns>
    ///     A FavoriteSourceResource assembled from the FavoriteSource
    /// </returns>
    public static FavoriteSourceResource ToResourceFromEntity(FavoriteSource entity) =>
        new(entity.Id, entity.NewsApiKey.Value, entity.SourceId.Value);
}