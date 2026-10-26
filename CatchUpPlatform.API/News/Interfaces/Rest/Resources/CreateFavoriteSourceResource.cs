using System.ComponentModel.DataAnnotations;

namespace CatchUpPlatform.API.News.Interfaces.Rest.Resources;

/// <summary>
///     Represents the data required to create a favorite source.
/// </summary>
/// <param name="NewsApiKey">The News API Key provided by NewsAPI (required)</param>
/// <param name="SourceId">The source identifier from NewsAPI (required)</param>
public record CreateFavoriteSourceResource(
    [Required(ErrorMessage = "RequiredNewsApiKey")]
    string NewsApiKey,
    [Required(ErrorMessage = "RequiredSourceId")]
    string SourceId);