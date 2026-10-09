namespace urlshortenerapi.Infraestructure.Http
{
    public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
}
