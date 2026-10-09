namespace urlshortenerapi.Features.Auth
{
    public static class AuthEndpoints
    {
        /// <summary>
        /// Mapea los endpoints de autenticación y autorización en la aplicación.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app) 
        {
            app.MapGroup("/api/auth")
                .WithTags("Auth")
                .MapIdentityApi<ApplicationUser>();

            return app;
        }
    }
}
