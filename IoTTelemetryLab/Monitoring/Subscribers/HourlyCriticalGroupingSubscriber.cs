using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Monitoring.Subscribers;

/// <summary>
/// ЛР2, варіант 5 (Завдання Б): групує критичні покази за годиною доби (Timestamp.Hour).
/// </summary>
public class HourlyCriticalGroupingSubscriber
{
    // SortedDictionary — групи одразу впорядковані за годиною, без окремого OrderBy при виводі
    private readonly SortedDictionary<int, List<TelemetryReading>> _byHour = new();

    /// <summary>Групи «година доби → критичні покази цієї години» у порядку зростання години.</summary>
    public IEnumerable<(int Hour, IReadOnlyList<TelemetryReading> Readings)> Groups =>
        _byHour.Select(kv => (kv.Key, (IReadOnlyList<TelemetryReading>)kv.Value));

    public void OnStatusEvaluated(TelemetryReading reading, SafetyStatus status)
    {
        if (status != SafetyStatus.Critical)
        {
            return;
        }

        int hour = reading.Timestamp.Hour;
        if (!_byHour.TryGetValue(hour, out var group))
        {
            group = new List<TelemetryReading>();
            _byHour[hour] = group;
        }

        group.Add(reading);
    }
}
