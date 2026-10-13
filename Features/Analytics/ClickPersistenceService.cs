using Microsoft.Extensions.Options;
using System.Threading.Channels;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Features.Analytics
{
    public sealed class ClickPersistenceService (ClickQueue queue,
    IServiceScopeFactory scopeFactory,
    IOptions<ClickTrackingOptions> options,
    ILogger<ClickPersistenceService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var batchSize = options.Value.BatchSize;
            var flushInterval = TimeSpan.FromSeconds(options.Value.FlushIntervalSeconds);
            var batch = new List<ClickRecord>(batchSize);
            var reader = queue.Reader;

            try
            {
                // Espera sin consumir CPU hasta que llegue al menos un clic.
                while (await reader.WaitToReadAsync(stoppingToken))
                {
                    await FillBatchAsync(reader, batch, batchSize, flushInterval, stoppingToken);
                    await FlushAsync(batch);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Apagado normal de la aplicación: se continúa abajo para no perder lo pendiente.
            }

            // Guarda lo que quedó en el lote y en la cola antes de terminar.
            while (reader.TryRead(out var click))
            {
                batch.Add(click);
                if (batch.Count >= batchSize)
                    await FlushAsync(batch);
            }

            await FlushAsync(batch);
            logger.LogInformation("Servicio de clics detenido. Clics pendientes guardados.");
        }

        /// <summary>Junta clics hasta llenar el lote o hasta que se cumpla el intervalo, lo que ocurra primero.</summary>
        private static async Task FillBatchAsync(
            ChannelReader<ClickRecord> reader,
            List<ClickRecord> batch,
            int batchSize,
            TimeSpan flushInterval,
            CancellationToken stoppingToken)
        {
            using var windowCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            windowCts.CancelAfter(flushInterval);

            try
            {
                while (batch.Count < batchSize)
                {
                    while (batch.Count < batchSize && reader.TryRead(out var click))
                        batch.Add(click);

                    if (batch.Count >= batchSize)
                        return;

                    if (!await reader.WaitToReadAsync(windowCts.Token))
                        return; // el canal se cerró
                }
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Se cumplió el intervalo: se guarda lo acumulado aunque el lote no esté lleno.
            }
        }

        private async Task FlushAsync(List<ClickRecord> batch)
        {
            if (batch.Count > 0)
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    db.ClickEvents.AddRange(batch.Select(c =>
                        new ClickEvent(c.LinkId, c.OccurredAt, c.RefererHost, c.UserAgent, c.IpHash)));

                    // CancellationToken.None: un guardado ya iniciado debe terminar aunque la app se esté apagando.
                    await db.SaveChangesAsync(CancellationToken.None);

                    logger.LogDebug("Guardados {Count} clics.", batch.Count);
                }
                catch (Exception ex)
                {
                    // Una excepción no controlada en un BackgroundService detiene TODA la aplicación (comportamiento por defecto desde .NET 6).
                    // Por eso aquí se captura todo: perder un lote de estadísticas es preferible a tumbar la API.
                    logger.LogError(ex, "No se pudieron guardar {Count} clics. El lote se descarta.", batch.Count);
                }
                finally
                {
                    batch.Clear();
                }
            }

            var dropped = queue.ResetDroppedCount();
            if (dropped > 0)
                logger.LogWarning("Se descartaron {Dropped} clics porque la cola estaba llena.", dropped);
        }
    }
}
