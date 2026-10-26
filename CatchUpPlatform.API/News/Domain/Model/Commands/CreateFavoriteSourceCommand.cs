using CatchUpPlatform.API.News.Domain.Model.ValueObjects;

namespace CatchUpPlatform.API.News.Domain.Model.Commands;

public record CreateFavoriteSourceCommand(NewsApiKey NewsApiKey, SourceId SourceId);