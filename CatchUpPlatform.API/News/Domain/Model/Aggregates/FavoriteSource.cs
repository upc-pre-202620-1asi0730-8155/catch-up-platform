using CatchUpPlatform.API.News.Domain.Model.Commands;
using CatchUpPlatform.API.News.Domain.Model.ValueObjects;

namespace CatchUpPlatform.API.News.Domain.Model.Aggregates;

public partial class FavoriteSource
{
    public int Id { get; private set; }
    public NewsApiKey NewsApiKey { get; private set; }
    public SourceId SourceId { get; private set; }

    protected FavoriteSource()
    {
        NewsApiKey = null!;
        SourceId = null!;
    }
    
    public FavoriteSource(CreateFavoriteSourceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        NewsApiKey = command.NewsApiKey;
        SourceId = command.SourceId;
    }
}