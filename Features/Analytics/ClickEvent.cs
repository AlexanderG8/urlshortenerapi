namespace urlshortenerapi.Features.Analytics
{
    public sealed class ClickEvent
    {
        public const int RefererHostMaxLength = 255;
        public const int UserAgentMaxLength = 512;
        public const int IpHashLength = 32; // HMAC-SHA256 = 32 bytes

        private ClickEvent() { }

        public ClickEvent(
            long shortLinkId,
            DateTimeOffset occurredAt,
            string? refererHost,
            string? userAgent,
            byte[]? ipHash)
        {
            if (ipHash is not null && ipHash.Length != IpHashLength)
                throw new ArgumentException($"El hash de IP debe tener {IpHashLength} bytes.", nameof(ipHash));

            ShortLinkId = shortLinkId;
            OccurredAt = occurredAt;
            RefererHost = Truncate(refererHost, RefererHostMaxLength);
            UserAgent = Truncate(userAgent, UserAgentMaxLength);
            IpHash = ipHash;
        }

        public long Id { get; private set; }
        public long ShortLinkId { get; private set; }
        public DateTimeOffset OccurredAt { get; private set; }
        public string? RefererHost { get; private set; }
        public string? UserAgent { get; private set; }
        public byte[]? IpHash { get; private set; }

        private static string? Truncate(string? value, int maxLength) =>
            string.IsNullOrWhiteSpace(value) ? null
            : value.Length <= maxLength ? value
            : value[..maxLength];
    }
}
