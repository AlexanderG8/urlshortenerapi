namespace urlshortenerapi.Features.Analytics.Endpoints
{
    public static class AnalyticsEndpoints
    {
        public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/links")
                .WithTags("Analytics")
                .RequireAuthorization();

            // maxlength(30) = ShortLinkConstraints.CodeMaxLength
            group.MapGet("/{code:maxlength(30)}/stats", GetLinkStats.HandleAsync)
                .WithName("GetLinkStats")
                .WithSummary("Estadísticas de clics de un enlace en un rango de fechas");

            return app;
        }
    }
}
