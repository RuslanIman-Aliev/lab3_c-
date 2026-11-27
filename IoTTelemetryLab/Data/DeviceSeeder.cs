using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

namespace IoTTelemetryLab.Data;

public static class DeviceSeeder
{
    public static readonly string[] Sources = { "вентилятор", "двигун", "фон" };

    public static async Task<int> SeedAsync(ITelemetryRepository repository, string deviceName,
        int readingCount, double minValue, double maxValue, CancellationToken ct = default)
    {
        var device = await repository.AddDeviceAsync(deviceName, ct);

        var raw = TelemetryGenerator.GenerateRawBatch(deviceName, readingCount, minValue, maxValue);
        var cleaned = TelemetryCleaner.RemoveInvalid(raw).ToList();

        var records = cleaned.Select(r => new TelemetryRecord
        {
            Value = r.Value,
            Timestamp = r.Timestamp,
            Source = Sources[Random.Shared.Next(Sources.Length)]
        }).ToList();

        await repository.AddReadingsAsync(device.Id, records, ct);
        return records.Count;
    }
}
