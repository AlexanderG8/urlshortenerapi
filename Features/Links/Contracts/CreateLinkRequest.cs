using System.ComponentModel.DataAnnotations;

namespace urlshortenerapi.Features.Links.Contracts
{
    public sealed record CreateLinkRequest
    {
        [Required]
        [StringLength(ShortLinkConstraints.DestinationUrlMaxLength)]
        public string DestinationUrl { get; init; } = string.Empty;

        /// <summary>Opcional. Si no se envía, se genera un código aleatorio.</summary>
        [StringLength(ShortLinkConstraints.CodeMaxLength)]
        public string? Alias { get; init; }

        /// <summary>Opcional. Fecha y hora con zona horaria, por ejemplo 2026-12-31T23:59:59-05:00.</summary>
        public DateTimeOffset? ExpiresAt { get; init; }
    }
}
