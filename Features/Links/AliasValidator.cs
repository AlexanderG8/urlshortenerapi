using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace urlshortenerapi.Features.Links
{
    public sealed class AliasValidator : IAliasValidator
    {
        private const int MinLength = 3;

        private readonly int _maxLength;
        private readonly HashSet<string> _reserved;

        public AliasValidator(IOptions<ShortLinkOptions> options)
        {
            _maxLength = options.Value.MaxAliasLength;
            _reserved = new HashSet<string>(options.Value.ReservedAliases, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsValid(string? alias, [NotNullWhen(false)] out string? error)
        {
            if (string.IsNullOrEmpty(alias))
            {
                error = "El alias no puede estar vacío.";
                return false;
            }

            if (alias.Length < MinLength || alias.Length > _maxLength)
            {
                error = $"El alias debe tener entre {MinLength} y {_maxLength} caracteres.";
                return false;
            }

            foreach (var c in alias)
            {
                if (!char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_'))
                {
                    error = "El alias solo puede contener letras sin tilde, números, guion (-) y guion bajo (_).";
                    return false;
                }
            }

            if (_reserved.Contains(alias))
            {
                error = "Ese alias está reservado por el sistema.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
