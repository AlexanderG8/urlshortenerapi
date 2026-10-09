using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Features.Links.Contracts;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Links.Endpoints
{
    internal static class CreateLink
    {
        private const int MaxGenerationAttempts = 3;

        public static async Task<Results<CreatedAtRoute<LinkResponse>, ValidationProblem, ProblemHttpResult>> HandleAsync(
            CreateLinkRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IDestinationUrlValidator urlValidator,
            IAliasValidator aliasValidator,
            IShortCodeGenerator codeGenerator,
            IOptions<ShortLinkOptions> options,
            TimeProvider timeProvider,
            CancellationToken cancellationToken)
        {
            var now = timeProvider.GetUtcNow();
            var expiresAt = request.ExpiresAt?.ToUniversalTime();

            var errors = LinkRequestValidation.ValidateDestinationAndExpiration(
                request.DestinationUrl, expiresAt, now, urlValidator, out var destinationUrl);

            if (request.Alias is not null && !aliasValidator.IsValid(request.Alias, out var aliasError))
                errors["alias"] = [aliasError];

            if (errors.Count > 0)
                return TypedResults.ValidationProblem(errors);

            var ownerId = user.GetRequiredUserId();
            ShortLink link;

            if (request.Alias is not null)
            {
                // Alias elegido por el usuario: si choca, es un conflicto que el cliente debe resolver.
                link = new ShortLink(request.Alias, destinationUrl!, ownerId, now, expiresAt);

                if (!await TrySaveAsync(db, link, cancellationToken))
                {
                    return TypedResults.Problem(
                        title: "El alias ya está en uso.",
                        detail: $"El alias '{request.Alias}' no está disponible. Elige otro.",
                        statusCode: StatusCodes.Status409Conflict);
                }
            }
            else
            {
                // Código generado: si choca (muy improbable), se reintenta con otro.
                for (var attempt = 1; ; attempt++)
                {
                    link = new ShortLink(codeGenerator.Generate(), destinationUrl!, ownerId, now, expiresAt);

                    if (await TrySaveAsync(db, link, cancellationToken))
                        break;

                    if (attempt == MaxGenerationAttempts)
                        throw new InvalidOperationException(
                            $"No se pudo generar un código único tras {MaxGenerationAttempts} intentos.");
                }
            }

            var response = LinkResponse.FromEntity(link, options.Value.BaseUrl);
            return TypedResults.CreatedAtRoute(response, GetLink.RouteName, new { code = link.Code });
        }

        private static async Task<bool> TrySaveAsync(AppDbContext db, ShortLink link, CancellationToken cancellationToken)
        {
            db.ShortLinks.Add(link);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                // Se quita la entidad fallida del seguimiento para que el siguiente intento no la reenvíe.
                db.Entry(link).State = EntityState.Detached;
                return false;
            }
        }
    }
}
