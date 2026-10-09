using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using urlshortenerapi.Features.Auth;
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
        CancellationToken cancellationToken)
        {
            var ownerId = user.GetRequiredUserId();

            var link = await db.ShortLinks
                .FirstOrDefaultAsync(l => l.Code == code && l.OwnerId == ownerId, cancellationToken);

            if (link is null)
                return TypedResults.NotFound();

            link.Deactivate(timeProvider.GetUtcNow());
            await db.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        }
    }
}
