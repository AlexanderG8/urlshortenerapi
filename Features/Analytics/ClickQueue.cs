using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace urlshortenerapi.Features.Analytics
{
    public sealed class ClickQueue
    {
        private readonly Channel<ClickRecord> _channel;
        private long _droppedCount;

        public ClickQueue(IOptions<ClickTrackingOptions> options)
        {
            // Se crea un canal acotado para almacenar los clics. Si el canal está lleno, las escrituras se bloquean hasta que haya espacio disponible.
            _channel = Channel.CreateBounded<ClickRecord>(new BoundedChannelOptions(options.Value.ChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait, // si el canal está lleno, las escrituras se bloquean hasta que haya espacio disponible
                SingleReader = true,   // solo ClickPersistenceService lee
                SingleWriter = false   // muchas peticiones escriben a la vez
            });
        }

        public ChannelReader<ClickRecord> Reader => _channel.Reader;

        public bool TryEnqueue(ClickRecord click)
        {
            if (_channel.Writer.TryWrite(click))
                return true;

            Interlocked.Increment(ref _droppedCount);
            return false;
        }

        /// <summary>Devuelve cuántos clics se descartaron desde la última llamada y reinicia el contador.</summary>
        public long ResetDroppedCount() => Interlocked.Exchange(ref _droppedCount, 0);
    }
}
