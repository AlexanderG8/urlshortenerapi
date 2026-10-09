using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Redirect
{
    public sealed class ShortLinkCache(HybridCache cache, IServiceScopeFactory scopeFactory)
    {
        private static readonly HybridCacheEntryOptions EntryOptions = new()
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration = TimeSpan.FromMinutes(10)
        };

        public ValueTask<RedirectTarget> GetAsync(string code, CancellationToken cancellationToken) =>
            cache.GetOrCreateAsync(
                Key(code),
                (code, scopeFactory),
                static async (state, token) =>
                {
                    // Scope propio: varias peticiones pueden compartir esta consulta (protección contra estampida),
                    // así que no puede depender del DbContext de una petición que podría terminar antes.
                    await using var scope = state.scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    return await db.ShortLinks
                        .AsNoTracking()
                        .Where(l => l.Code == state.code)
                        .Select(l => new RedirectTarget(l.Id, l.DestinationUrl, l.IsActive, l.ExpiresAt))
                        .FirstOrDefaultAsync(token)
                        ?? RedirectTarget.NotFound;
                },
                EntryOptions,
                cancellationToken: cancellationToken);

        public ValueTask InvalidateAsync(string code, CancellationToken cancellationToken = default) =>
            cache.RemoveAsync(Key(code), cancellationToken);

        private static string Key(string code) => $"redirect:{code}";
    }
}
