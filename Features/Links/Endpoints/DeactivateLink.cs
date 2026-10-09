using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Features.Redirect;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Links.Endpoints
{
    internal static class DeactivateLink
    {
        public static async Task<Results<NoContent, NotFound>> HandleAsync(
        string code,
        ClaimsPrincipal user,
        AppDbContext db,
        TimeProvider timeProvider,
        ShortLinkCache linkCache,
        CancellationToken cancellationToken)
        {
            var ownerId = user.GetRequiredUserId();

            var link = await db.ShortLinks
                .FirstOrDefaultAsync(l => l.Code == code && l.OwnerId == ownerId, cancellationToken);

            if (link is null)
                return TypedResults.NotFound();

            link.Deactivate(timeProvider.GetUtcNow());
            await db.SaveChangesAsync(cancellationToken);
            await linkCache.InvalidateAsync(link.Code, cancellationToken);
            return TypedResults.NoContent();
        }
    }
}
