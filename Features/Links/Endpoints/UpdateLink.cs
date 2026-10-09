using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Features.Links.Contracts;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Links.Endpoints
{
    internal static class UpdateLink
    {
        public static async Task<Results<Ok<LinkResponse>, ValidationProblem, NotFound, ProblemHttpResult>> HandleAsync(
        string code,
        UpdateLinkRequest request,
        ClaimsPrincipal user,
        AppDbContext db,
        IDestinationUrlValidator urlValidator,
        IOptions<ShortLinkOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
        {
            var now = timeProvider.GetUtcNow();
            var expiresAt = request.ExpiresAt?.ToUniversalTime();

            var errors = LinkRequestValidation.ValidateDestinationAndExpiration(
                request.DestinationUrl, expiresAt, now, urlValidator, out var destinationUrl);

            if (errors.Count > 0)
                return TypedResults.ValidationProblem(errors);

            var ownerId = user.GetRequiredUserId();

            var link = await db.ShortLinks
                .FirstOrDefaultAsync(l => l.Code == code && l.OwnerId == ownerId, cancellationToken);

            if (link is null)
                return TypedResults.NotFound();

            if (!link.IsActive)
            {
                return TypedResults.Problem(
                    title: "El enlace está desactivado.",
                    detail: "Los enlaces desactivados no se pueden modificar.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            link.Update(destinationUrl!, expiresAt, now);
            await db.SaveChangesAsync(cancellationToken);

            return TypedResults.Ok(LinkResponse.FromEntity(link, options.Value.BaseUrl));
        }
    }
}
