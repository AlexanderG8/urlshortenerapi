using System.Diagnostics.CodeAnalysis;

namespace urlshortenerapi.Features.Links
{
    public interface IAliasValidator
    {
        bool IsValid(string? alias, [NotNullWhen(false)] out string? error);
    }
}
