namespace urlshortenerapi.Features.Redirect
{
    public static class RedirectEndpoints
    {
        public static IEndpointRouteBuilder MapRedirectEndpoints(this IEndpointRouteBuilder app)
        {
            // maxlength(30) = ShortLinkConstraints.CodeMaxLength
            app.MapGet("/{code:maxlength(30)}", RedirectToDestination.HandleAsync)
                .AllowAnonymous()
                .WithName("RedirectToDestination")
                .WithTags("Redirect")
                .WithSummary("Redirige al destino del enlace corto");

            return app;
        }
    }
}
