using System.ComponentModel;
using System.Text.Json.Serialization;

namespace urlshortenerapi.Features.Redirect
{
    // ImmutableObject + sealed: HybridCache puede devolver la misma instancia desde memoria
    // en lugar de crear una copia en cada lectura.
    [ImmutableObject(true)]
    public sealed record RedirectTarget(
        long LinkId,
        string? DestinationUrl,
        bool IsActive,
        DateTimeOffset? ExpiresAt)
    {
        /// <summary>Marcador para códigos inexistentes. Se guarda en caché para no consultar la base de datos cada vez.</summary>
        public static readonly RedirectTarget NotFound = new(0, null, false, null);

        [JsonIgnore]
        public bool Exists => LinkId != 0;

        public bool IsExpired(DateTimeOffset now) => ExpiresAt is not null && ExpiresAt <= now;
    }
}
