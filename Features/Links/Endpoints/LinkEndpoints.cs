namespace urlshortenerapi.Features.Links.Endpoints
{
    public static class LinkEndpoints
    {
        // maxlength(30) = ShortLinkConstraints.CodeMaxLength. Un código más largo se rechaza con 404 sin consultar la base de datos.
        private const string CodeRoute = "/{code:maxlength(30)}";

        public static IEndpointRouteBuilder MapLinkEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/links")
                .WithTags("Links")
                .RequireAuthorization();

            group.MapPost("/", CreateLink.HandleAsync)
                .WithName("CreateLink")
                .WithSummary("Crea un enlace corto");

            group.MapGet("/", ListLinks.HandleAsync)
                .WithName("ListLinks")
                .WithSummary("Lista los enlaces del usuario autenticado");

            group.MapGet(CodeRoute, GetLink.HandleAsync)
                .WithName(GetLink.RouteName)
                .WithSummary("Obtiene un enlace por su código");

            group.MapPut(CodeRoute, UpdateLink.HandleAsync)
                .WithName("UpdateLink")
                .WithSummary("Actualiza el destino y la expiración de un enlace");

            group.MapDelete(CodeRoute, DeactivateLink.HandleAsync)
                .WithName("DeactivateLink")
                .WithSummary("Desactiva un enlace (conserva sus estadísticas)");

            return app;
        }
    }
}
