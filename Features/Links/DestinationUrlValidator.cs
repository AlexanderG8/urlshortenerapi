using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace urlshortenerapi.Features.Links
{
    public class DestinationUrlValidator : IDestinationUrlValidator
    {
        private readonly int _maxLength;
        private readonly string _ownHost;
        private readonly string[] _blockedDomains;

        public DestinationUrlValidator(IOptions<ShortLinkOptions> options)
        {
            _maxLength = options.Value.MaxUrlLength;
            _ownHost = new Uri(options.Value.BaseUrl).IdnHost;
            _blockedDomains = options.Value.BlockedDomains;
        }

        public bool TryNormalize(
            string? input,
            [NotNullWhen(true)] out string? normalizedUrl,
            [NotNullWhen(false)] out string? error)
        {
            normalizedUrl = null;

            if (string.IsNullOrWhiteSpace(input))
                return Fail("La URL es obligatoria.", out error);

            var trimmed = input.Trim();

            if (trimmed.Length > _maxLength)
                return Fail($"La URL no puede superar los {_maxLength} caracteres.", out error);

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
                return Fail("La URL debe ser absoluta, por ejemplo https://ejemplo.com.", out error);

            // Bloquea javascript:, data:, file:, ftp:, etc.
            // En Linux, "/ruta" se interpreta como file:///ruta, así que este control también cubre ese caso.
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return Fail("Solo se permiten URLs http o https.", out error);

            // "https://banco.com@sitio-malicioso.com" lleva al segundo dominio: truco clásico de phishing.
            if (!string.IsNullOrEmpty(uri.UserInfo))
                return Fail("La URL no puede contener usuario ni contraseña.", out error);

            if (uri.IsLoopback)
                return Fail("No se permiten direcciones locales.", out error);

            // IdnHost convierte dominios internacionales a Punycode para compararlos de forma fiable.
            var host = uri.IdnHost;

            if (IsSameOrSubdomain(host, _ownHost))
                return Fail("No se puede acortar un enlace de este mismo servicio.", out error);

            if (_blockedDomains.Any(blocked => IsSameOrSubdomain(host, blocked)))
                return Fail("El dominio de destino no está permitido.", out error);

            var normalized = uri.AbsoluteUri;

            // La normalización puede codificar caracteres y alargar la URL.
            if (normalized.Length > _maxLength)
                return Fail($"La URL no puede superar los {_maxLength} caracteres.", out error);

            normalizedUrl = normalized;
            error = null;
            return true;
        }

        private static bool IsSameOrSubdomain(string host, string domain) =>
            host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);

        private static bool Fail(string message, out string? error)
        {
            error = message;
            return false;
        }
    }
}
