namespace urlshortenerapi.Features.Analytics
{
    /// <summary>Datos de un clic esperando en la cola para guardarse.</summary>
    public readonly record struct ClickRecord(
    long LinkId,
    DateTimeOffset OccurredAt,
    string? RefererHost,
    string? UserAgent,
    byte[]? IpHash);
}
