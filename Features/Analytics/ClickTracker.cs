using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace urlshortenerapi.Features.Analytics
{
    public class ClickTracker : IClickTracker
    {
        private readonly ClickQueue _queue;
        private readonly TimeProvider _timeProvider;
        private readonly byte[] _ipHashKey;

        public ClickTracker(ClickQueue queue, IOptions<ClickTrackingOptions> options, TimeProvider timeProvider)
        {
            _queue = queue;
            _timeProvider = timeProvider;
            _ipHashKey = Encoding.UTF8.GetBytes(options.Value.IpHashKey);
        }

        public void Track(HttpContext httpContext, long linkId)
        {
            // Se extrae todo de HttpContext aquí, de forma síncrona.
            // HttpContext no puede usarse después de que la petición termina, así que nunca debe llegar a la cola.
            var request = httpContext.Request;
            var userAgent = request.Headers.UserAgent.ToString();

            var click = new ClickRecord(
                LinkId: linkId,
                OccurredAt: _timeProvider.GetUtcNow(),
                RefererHost: GetRefererHost(request.Headers.Referer.ToString()),
                UserAgent: string.IsNullOrWhiteSpace(userAgent) ? null : userAgent,
                IpHash: HashIp(httpContext.Connection.RemoteIpAddress));

            _queue.TryEnqueue(click);
        }

        private static string? GetRefererHost(string? referer)
        {
            if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri))
                return null;

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps
                ? uri.IdnHost
                : null;
        }

        private byte[]? HashIp(IPAddress? ip)
        {
            if (ip is null)
                return null;

            // "Ejemplo: Si 192.168.1.10" y "192.168.1.10" son la misma IP: se normalizan para que generen el mismo hash.
            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            return HMACSHA256.HashData(_ipHashKey, ip.GetAddressBytes());
        }
    }
}
