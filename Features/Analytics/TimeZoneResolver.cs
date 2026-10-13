using System.Diagnostics.CodeAnalysis;

namespace urlshortenerapi.Features.Analytics
{
    internal static class TimeZoneResolver
    {
        /// <summary>
        /// Resuelve un ID de zona horaria (IANA o Windows) y obtiene el nombre que entiende SQL Server.
        /// </summary>
        public static bool TryResolve(
            string? timeZoneId,
            [NotNullWhen(true)] out TimeZoneInfo? timeZone,
            [NotNullWhen(true)] out string? sqlServerId)
        {
            sqlServerId = null;
            var id = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId.Trim();

            // TryFindSystemTimeZoneById: .NET 8+. Acepta IDs IANA y de Windows.
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(id, out timeZone))
                return false;

            if (timeZone.HasIanaId)
                return TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZone.Id, out sqlServerId);

            sqlServerId = timeZone.Id;
            return true;
        }

        /// <summary>Instante UTC en que empieza un día en la zona indicada.</summary>
        public static DateTimeOffset StartOfDayUtc(DateOnly day, TimeZoneInfo timeZone)
        {
            var local = day.ToDateTime(TimeOnly.MinValue);

            // En algunas zonas el cambio de horario ocurre a medianoche y las 00:00 no existen ese día.
            while (timeZone.IsInvalidTime(local))
                local = local.AddMinutes(30);

            return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, timeZone), TimeSpan.Zero);
        }

        public static DateOnly Today(DateTimeOffset utcNow, TimeZoneInfo timeZone) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utcNow, timeZone).DateTime);
    }
}
