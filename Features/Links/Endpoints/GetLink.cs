using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Features.Links.Contracts;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Links.Endpoints
{
    internal static class GetLink
    {
        public const string RouteName = "GetLink";

        public static async Task<Results<Ok<LinkResponse>, NotFound>> HandleAsync(
            string code,
            ClaimsPrincipal user,
            AppDbContext db,
            IOptions<ShortLinkOptions> options,
            CancellationToken cancellationToken)
        {
            var ownerId = user.GetRequiredUserId();

            var link = await db.ShortLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Code == code && l.OwnerId == ownerId, cancellationToken);

            return link is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(LinkResponse.FromEntity(link, options.Value.BaseUrl));
        }
    }
}
