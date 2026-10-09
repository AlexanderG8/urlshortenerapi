using System.ComponentModel.DataAnnotations;

namespace urlshortenerapi.Features.Analytics
{
    public sealed class ClickTrackingOptions
    {
        public const string SectionName = "ClickTracking";

        /// <summary>Máximo de clics en memoria esperando a guardarse. Si se llena, se descartan (la redirección nunca espera).</summary>
        [Range(100, 100_000)]
        public int ChannelCapacity { get; init; } = 10_000;

        /// <summary>Cantidad de clics que se insertan por lote.</summary>
        [Range(1, 1_000)]
        public int BatchSize { get; init; } = 100;

        /// <summary>Segundos máximos que un clic espera en memoria antes de guardarse.</summary>
        [Range(1, 60)]
        public int FlushIntervalSeconds { get; init; } = 5;

        /// <summary>Clave secreta para hashear IPs (HMAC). Va en User Secrets, nunca en appsettings.</summary>
        [Required, MinLength(32)]
        public string IpHashKey { get; init; } = string.Empty;
    }
}
