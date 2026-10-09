using System.Diagnostics.CodeAnalysis;

namespace urlshortenerapi.Features.Links
{
    public interface IDestinationUrlValidator
    {
        /// <summary>Valida la URL y devuelve su forma normalizada, que es la que debe guardarse.</summary>
        bool TryNormalize(
            string? input,
            [NotNullWhen(true)] out string? normalizedUrl,
            [NotNullWhen(false)] out string? error);
    }
}
