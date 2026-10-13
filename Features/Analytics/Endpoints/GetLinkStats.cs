using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using urlshortenerapi.Features.Analytics.Contracts;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Analytics.Endpoints
{
    public class GetLinkStats
    {
        private const int DefaultRangeDays = 30;
        private const int MaxRangeDays = 366;

        public static async Task<Results<Ok<LinkStatsResponse>, ValidationProblem, NotFound>> HandleAsync(
            string code,
            ClaimsPrincipal user,
            HttpContext httpContext,
            AppDbContext db,
            LinkStatsQuery statsQuery,
            TimeProvider timeProvider,
            CancellationToken cancellationToken,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery, StringLength(64)] string? timeZone = null)
        {
            // 1. Zona horaria
            if (!TimeZoneResolver.TryResolve(timeZone, out var tz, out var sqlServerTimeZone))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["timeZone"] = ["Zona horaria no reconocida. Usa un ID IANA, por ejemplo America/Lima."]
                });
            }

            // 2. Rango de fechas
            var toDate = to ?? TimeZoneResolver.Today(timeProvider.GetUtcNow(), tz);
            var fromDate = from ?? toDate.AddDays(-(DefaultRangeDays - 1));

            if (fromDate > toDate)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["from"] = ["La fecha inicial no puede ser posterior a la final."]
                });
            }

            if (toDate.DayNumber - fromDate.DayNumber + 1 > MaxRangeDays)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["to"] = [$"El rango no puede superar los {MaxRangeDays} días."]
                });
            }

            // 3. Propiedad del enlace: SIEMPRE antes de tocar la caché de estadísticas.
            var ownerId = user.GetRequiredUserId();

            var linkId = await db.ShortLinks
                .Where(l => l.Code == code && l.OwnerId == ownerId)
                .Select(l => (long?)l.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (linkId is null)
                return TypedResults.NotFound();

            // 4. Estadísticas (desde caché o calculadas)
            var range = new StatsRange(
                fromDate,
                toDate,
                TimeZoneResolver.StartOfDayUtc(fromDate, tz),
                TimeZoneResolver.StartOfDayUtc(toDate.AddDays(1), tz),
                sqlServerTimeZone);

            var data = await statsQuery.GetAsync(linkId.Value, range, cancellationToken);

            // "private": solo el navegador del usuario puede guardarla, nunca un proxy compartido.
            httpContext.Response.Headers.CacheControl = "private, max-age=60";

            return TypedResults.Ok(new LinkStatsResponse(
                code,
                timeZone?.Trim() is { Length: > 0 } requested ? requested : "UTC",
                fromDate,
                toDate,
                data.TotalClicks,
                data.UniqueVisitors,
                data.ClicksByDay,
                data.TopReferers));
        }
    }
}
