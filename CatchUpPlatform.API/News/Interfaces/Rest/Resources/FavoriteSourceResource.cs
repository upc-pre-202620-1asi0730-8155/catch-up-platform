namespace CatchUpPlatform.API.News.Interfaces.Rest.Resources;

/// <summary>
///     Represents the data provided by the server about a favorite source.
/// </summary>
/// <param name="Id">The server-generated ID of the favorite source</param>
/// <param name="NewsApiKey">The News API Key provided by NewsAPI</param>
/// <param name="SourceId">The source identifier from NewsAPI</param>
public record FavoriteSourceResource(
    int Id,
    string NewsApiKey,
    string SourceId);