using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

/// <summary>
/// Очищення потоку телеметрії від некоректних показів.
/// </summary>
public static class TelemetryCleaner
{
    private const double ErrorMarker = -999.0;

    /// <summary>
    /// Відфільтровує покази з апаратними помилками: NaN (шум АЦП) та -999 (обрив контакту).
    /// </summary>
    /// <param name="raw">«Сирий» потік показів.</param>
    /// <returns>Відкладена (deferred) послідовність лише коректних показів.</returns>
    public static IEnumerable<TelemetryReading> RemoveInvalid(
        IEnumerable<TelemetryReading> raw)
    {
        return raw.Where(r =>
            !double.IsNaN(r.Value) &&
            r.Value != ErrorMarker);
    }
}
