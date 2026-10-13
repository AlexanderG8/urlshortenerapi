using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using urlshortenerapi.Features.Analytics.Contracts;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Analytics
{
    /// <summary>Rango ya validado: fechas locales y sus límites en UTC.</summary>
    public sealed record StatsRange(
        DateOnly From,
        DateOnly To,
        DateTimeOffset StartUtc,   // inclusivo
        DateTimeOffset EndUtc,     // exclusivo
        string SqlServerTimeZone);

    public sealed class LinkStatsQuery(HybridCache cache, IServiceScopeFactory scopeFactory)
    {
        private const int TopReferersCount = 10;

        private static readonly HybridCacheEntryOptions EntryOptions = new()
        {
            Expiration = TimeSpan.FromMinutes(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };

        public ValueTask<LinkStatsData> GetAsync(long linkId, StatsRange range, CancellationToken cancellationToken) =>
            cache.GetOrCreateAsync(
                $"stats:{linkId}:{range.From:yyyy-MM-dd}:{range.To:yyyy-MM-dd}:{range.SqlServerTimeZone}",
                (linkId, range, scopeFactory),
                static async (state, token) =>
                {
                    await using var scope = state.scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    return await ComputeAsync(db, state.linkId, state.range, token);
                },
                EntryOptions,
                cancellationToken: cancellationToken);

        private static async Task<LinkStatsData> ComputeAsync(
            AppDbContext db, long linkId, StatsRange range, CancellationToken token)
        {
            var start = range.StartUtc;
            var end = range.EndUtc;
            var timeZone = range.SqlServerTimeZone;

            // Las tres consultas usan el mismo WHERE sobre el índice (ShortLinkId, OccurredAt) INCLUDE (RefererHost, IpHash).
            // No se aplica ninguna función a OccurredAt en el WHERE: así SQL Server puede usar el índice (consulta sargable).

            var summary = await db.Database.SqlQuery<SummaryRow>($"""
            SELECT COUNT(*) AS TotalClicks,
                   COUNT(DISTINCT c.IpHash) AS UniqueVisitors
            FROM dbo.ClickEvents AS c
            WHERE c.ShortLinkId = {linkId}
              AND c.OccurredAt >= {start}
              AND c.OccurredAt < {end}
            """).SingleAsync(token);

            // CROSS APPLY calcula el día local una sola vez. Si se repitiera la expresión en SELECT y GROUP BY,
            // cada {timeZone} sería un parámetro distinto (@p1, @p4) y SQL Server rechazaría el GROUP BY
            // por considerarlas expresiones diferentes.
            var dailyRows = await db.Database.SqlQuery<DailyRow>($"""
            SELECT d.[Day], COUNT(*) AS Clicks
            FROM dbo.ClickEvents AS c
            CROSS APPLY (SELECT CAST(c.OccurredAt AT TIME ZONE {timeZone} AS date) AS [Day]) AS d
            WHERE c.ShortLinkId = {linkId}
              AND c.OccurredAt >= {start}
              AND c.OccurredAt < {end}
            GROUP BY d.[Day]
            """).ToListAsync(token);

            var refererRows = await db.Database.SqlQuery<RefererRow>($"""
            SELECT TOP ({TopReferersCount}) c.RefererHost, COUNT(*) AS Clicks
            FROM dbo.ClickEvents AS c
            WHERE c.ShortLinkId = {linkId}
              AND c.OccurredAt >= {start}
              AND c.OccurredAt < {end}
            GROUP BY c.RefererHost
            ORDER BY COUNT(*) DESC, c.RefererHost
            """).ToListAsync(token);

            return new LinkStatsData(
                summary.TotalClicks,
                summary.UniqueVisitors,
                FillMissingDays(dailyRows, range.From, range.To),
                refererRows.Select(r => new RefererClicks(r.RefererHost, r.Clicks)).ToList());
        }

        /// <summary>Los días sin clics no aparecen en el GROUP BY; un gráfico necesita la serie completa con ceros.</summary>
        private static List<DailyClicks> FillMissingDays(List<DailyRow> rows, DateOnly from, DateOnly to)
        {
            var counts = rows.ToDictionary(r => r.Day, r => r.Clicks);
            var result = new List<DailyClicks>(to.DayNumber - from.DayNumber + 1);

            for (var day = from; day <= to; day = day.AddDays(1))
                result.Add(new DailyClicks(day, counts.GetValueOrDefault(day)));

            return result;
        }

        // Tipos para mapear los resultados de SQL. Los nombres de las propiedades deben coincidir con los alias de las columnas.
        private sealed class SummaryRow
        {
            public int TotalClicks { get; init; }
            public int UniqueVisitors { get; init; }
        }

        private sealed class DailyRow
        {
            public DateOnly Day { get; init; }
            public int Clicks { get; init; }
        }

        private sealed class RefererRow
        {
            public string? RefererHost { get; init; }
            public int Clicks { get; init; }
        }
    }
}
