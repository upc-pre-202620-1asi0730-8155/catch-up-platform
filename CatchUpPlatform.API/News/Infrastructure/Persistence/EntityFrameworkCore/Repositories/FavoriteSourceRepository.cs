using CatchUpPlatform.API.News.Domain.Model.Aggregates;
using CatchUpPlatform.API.News.Domain.Model.ValueObjects;
using CatchUpPlatform.API.News.Domain.Repositories;
using CatchUpPlatform.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using CatchUpPlatform.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CatchUpPlatform.API.News.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class FavoriteSourceRepository(AppDbContext context) : BaseRepository<FavoriteSource>(context), IFavoriteSourceRepository
{
    public async Task<IEnumerable<FavoriteSource>> FindByNewsApiKeyAsync(NewsApiKey newsApiKey, CancellationToken cancellationToken = default)
    {
        return await Context.Set<FavoriteSource>()
            .Where(f => f.NewsApiKey == newsApiKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<FavoriteSource?> FindByNewsApiKeyAndSourceIdAsync(NewsApiKey newsApiKey, SourceId sourceId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FavoriteSource>()
            .FirstOrDefaultAsync(f => f.NewsApiKey == newsApiKey && f.SourceId == sourceId, cancellationToken);
    }
}