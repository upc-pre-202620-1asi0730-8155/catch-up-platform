using CatchUpPlatform.API.News.Domain.Model.ValueObjects;

namespace CatchUpPlatform.API.News.Domain.Model.Queries;

public record GetAllFavoriteSourcesByNewsApiKeyQuery(NewsApiKey NewsApiKey);