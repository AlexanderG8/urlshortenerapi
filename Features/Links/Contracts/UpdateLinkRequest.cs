using System.ComponentModel.DataAnnotations;

namespace urlshortenerapi.Features.Links.Contracts
{
    public sealed record UpdateLinkRequest
    {
        [Required]
        [StringLength(ShortLinkConstraints.DestinationUrlMaxLength)]
        public string DestinationUrl { get; init; } = string.Empty;

        /// <summary>Si se envía null, el enlace deja de expirar.</summary>
        public DateTimeOffset? ExpiresAt { get; init; }
    }
}
