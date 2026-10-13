namespace urlshortenerapi.Features.Analytics.Contracts
{
    public sealed record LinkStatsResponse(
        string Code,
    string TimeZone,
    DateOnly From,
    DateOnly To,
    int TotalClicks,
    int UniqueVisitors,
    IReadOnlyList<DailyClicks> ClicksByDay,
    IReadOnlyList<RefererClicks> TopReferers);

    public sealed record DailyClicks(DateOnly Date, int Clicks);

    /// <summary>Referer null = acceso directo (sin referer: app, correo, URL escrita a mano).</summary>
    public sealed record RefererClicks(string? Referer, int Clicks);

    /// <summary>Datos calculados que se guardan en caché.</summary>
    public sealed record LinkStatsData(
        int TotalClicks,
        int UniqueVisitors,
        IReadOnlyList<DailyClicks> ClicksByDay,
        IReadOnlyList<RefererClicks> TopReferers);
}
