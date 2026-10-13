namespace urlshortenerapi.Features.Analytics
{
    public interface IClickTracker
    {
        void Track(HttpContext httpContext, long linkId);
    }
}
