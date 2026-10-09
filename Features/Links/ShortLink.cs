namespace urlshortenerapi.Features.Links
{
    public sealed class ShortLink
    {
        // Constructor requerido por EF Core para materializar registros.
        private ShortLink() { }

        public ShortLink(
            string code,
            string destinationUrl,
            string ownerId,
            DateTimeOffset createdAt,
            DateTimeOffset? expiresAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

            if (expiresAt is not null && expiresAt <= createdAt)
                throw new ArgumentException("La fecha de expiración debe ser posterior a la de creación.", nameof(expiresAt));

            Code = code;
            DestinationUrl = destinationUrl;
            OwnerId = ownerId;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
            IsActive = true;
        }

        public long Id { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string DestinationUrl { get; private set; } = string.Empty;
        public string OwnerId { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? ExpiresAt { get; private set; }

        public bool IsExpired(DateTimeOffset now) => ExpiresAt is not null && ExpiresAt <= now;

        public void Update(string destinationUrl, DateTimeOffset? expiresAt, DateTimeOffset now)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationUrl);

            if (expiresAt is not null && expiresAt <= now)
                throw new ArgumentException("La fecha de expiración debe ser futura.", nameof(expiresAt));

            DestinationUrl = destinationUrl;
            ExpiresAt = expiresAt;
            UpdatedAt = now;
        }

        public void Deactivate(DateTimeOffset now)
        {
            if (!IsActive) return;
            IsActive = false;
            UpdatedAt = now;
        }
    }
}
