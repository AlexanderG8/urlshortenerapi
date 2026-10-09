namespace urlshortenerapi.Features.Links.Endpoints
{
    internal static class LinkRequestValidation
    {
        public static Dictionary<string, string[]> ValidateDestinationAndExpiration(
        string destinationUrl,
        DateTimeOffset? expiresAt,
        DateTimeOffset now,
        IDestinationUrlValidator urlValidator,
        out string? normalizedUrl)
        {
            var errors = new Dictionary<string, string[]>();

            if (!urlValidator.TryNormalize(destinationUrl, out normalizedUrl, out var urlError))
                errors["destinationUrl"] = [urlError];

            if (expiresAt is not null && expiresAt <= now)
                errors["expiresAt"] = ["La fecha de expiración debe ser futura."];

            return errors;
        }
    }
}
