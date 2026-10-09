namespace urlshortenerapi.Features.Links.Contracts
{
    public sealed record LinkResponse(string Code, string ShortUrl, string DestinationUrl, bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, DateTimeOffset? ExpiresAt)
    {
        public static LinkResponse FromEntity(ShortLink link, string baseUrl) => new(
        link.Code,
        $"{baseUrl.TrimEnd('/')}/{link.Code}",
        link.DestinationUrl,
        link.IsActive,
        link.CreatedAt,
        link.UpdatedAt,
        link.ExpiresAt);
    }
}
