using System.Security.Claims;

namespace urlshortenerapi.Features.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        /// Devuelve el Id del usuario autenticado.
        /// Solo devo usarlo en endpoints que requieran autorización, de lo contrario lanzará una excepción.
        public static string GetRequiredUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException(
                    "No se encontró el Id del usuario. ¿Falta RequireAuthorization() en el endpoint?");
        }
    }
}
