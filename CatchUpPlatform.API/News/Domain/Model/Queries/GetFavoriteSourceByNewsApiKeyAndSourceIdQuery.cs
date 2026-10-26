using CatchUpPlatform.API.News.Domain.Model.ValueObjects;

namespace CatchUpPlatform.API.News.Domain.Model.Queries;

public record GetFavoriteSourceByNewsApiKeyAndSourceIdQuery(NewsApiKey NewsApiKey, SourceId SourceId);