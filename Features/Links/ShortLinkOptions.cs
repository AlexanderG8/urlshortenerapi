using System.ComponentModel.DataAnnotations;

namespace urlshortenerapi.Features.Links
{
    public sealed class ShortLinkOptions
    {
        public const string SectionName = "ShortLinks";

        /// <summary>URL pública con la que se construyen los enlaces cortos devueltos al cliente.</summary>
        [Required, Url]
        public string BaseUrl { get; init; } = string.Empty;

        /// <summary>Longitud de los códigos generados automáticamente.</summary>
        [Range(5, 12)]
        public int CodeLength { get; init; } = 7;

        [Range(3, ShortLinkConstraints.CodeMaxLength)]
        public int MaxAliasLength { get; init; } = ShortLinkConstraints.CodeMaxLength;

        [Range(100, ShortLinkConstraints.DestinationUrlMaxLength)]
        public int MaxUrlLength { get; init; } = ShortLinkConstraints.DestinationUrlMaxLength;

        /// <summary>Palabras que no se pueden usar como alias porque chocan con rutas de la API.</summary>
        public string[] ReservedAliases { get; init; } = [];

        /// <summary>Dominios a los que no se permite redirigir.</summary>
        public string[] BlockedDomains { get; init; } = [];
    }
}
