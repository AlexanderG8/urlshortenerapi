using Microsoft.AspNetCore.Http.HttpResults;
namespace urlshortenerapi.Features.Redirect;

internal static class RedirectToDestination
{
    public static async Task<Results<RedirectHttpResult, NotFound, ProblemHttpResult>> HandleAsync(
        string code,
        HttpContext httpContext,
        ShortLinkCache linkCache,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var target = await linkCache.GetAsync(code, cancellationToken);

        if (!target.Exists || !target.IsActive)
            return TypedResults.NotFound();

        if (target.IsExpired(timeProvider.GetUtcNow()))
        {
            return TypedResults.Problem(
                title: "El enlace expiró.",
                statusCode: StatusCodes.Status410Gone);
        }


        // Evita que navegadores o proxies guarden la redirección y dejen de pasar por la API.
        httpContext.Response.Headers.CacheControl = "no-store";

        return TypedResults.Redirect(target.DestinationUrl!);
    }
}
