using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace urlshortenerapi.Features.Links
{
    public sealed class ShortCodeGenerator(IOptions<ShortLinkOptions> options) : IShortCodeGenerator
    {
        public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        private readonly int _length = options.Value.CodeLength;

        public string Generate() => RandomNumberGenerator.GetString(Alphabet, _length);
    }
}
