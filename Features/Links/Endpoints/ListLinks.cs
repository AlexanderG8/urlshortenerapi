using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Features.Links.Contracts;
using urlshortenerapi.Infraestructure.Http;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Links.Endpoints
{
    internal static class ListLinks
    {
        public static async Task<Ok<PagedResponse<LinkResponse>>> HandleAsync(
        ClaimsPrincipal user,
        AppDbContext db,
        IOptions<ShortLinkOptions> options,
        CancellationToken cancellationToken,
        [FromQuery, Range(1, 10_000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
        {
            var ownerId = user.GetRequiredUserId();

            var query = db.ShortLinks
                .AsNoTracking()
                .Where(l => l.OwnerId == ownerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var links = await query
                .OrderByDescending(l => l.CreatedAt)
                .ThenByDescending(l => l.Id) // desempate: sin él, enlaces con la misma fecha podrían repetirse o saltarse entre páginas
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var baseUrl = options.Value.BaseUrl;
            var items = links.Select(l => LinkResponse.FromEntity(l, baseUrl)).ToList();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return TypedResults.Ok(new PagedResponse<LinkResponse>(items, page, pageSize, totalCount, totalPages));
        }
    }
}
