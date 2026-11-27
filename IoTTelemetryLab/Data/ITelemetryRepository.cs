using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Data;

public interface ITelemetryRepository
{
    Task<Device> AddDeviceAsync(string name, CancellationToken ct = default);
    Task AddReadingsAsync(int deviceId, IEnumerable<TelemetryRecord> readings, CancellationToken ct = default);

    Task<List<Device>> GetAllDevicesAsync(CancellationToken ct = default);
    Task<Device?> GetDeviceWithRecordsAsync(int id, CancellationToken ct = default);
    Task<int> CountRecordsAsync(int deviceId, CancellationToken ct = default);
    Task<List<DeviceSummary>> GetSummariesAsync(CancellationToken ct = default);

    Task<bool> RenameDeviceAsync(int id, string newName, CancellationToken ct = default);
    Task<bool> DeleteDeviceAsync(int id, CancellationToken ct = default);

    // Варіант 5, завдання А: частка показів вище порога для вказаного джерела
    Task<SourceLoudShare> GetLoudShareBySourceAsync(string source, double threshold, CancellationToken ct = default);

    // Варіант 5, завдання Б: пристрій лише з показами джерела вище порога, за спаданням значення
    Task<Device?> GetDeviceWithLoudRecordsAsync(int deviceId, string source, double threshold, CancellationToken ct = default);
}
