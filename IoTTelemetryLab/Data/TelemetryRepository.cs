using IoTTelemetryLab.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTTelemetryLab.Data;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly AppDbContext _context;

    public TelemetryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Device> AddDeviceAsync(string name, CancellationToken ct = default)
    {
        var device = new Device { Name = name };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync(ct);
        return device;
    }

    public async Task AddReadingsAsync(int deviceId, IEnumerable<TelemetryRecord> readings, CancellationToken ct = default)
    {
        // ToList() фіксує колекцію, щоб повторний перебір не створив нові об'єкти
        var list = readings.ToList();
        foreach (var r in list)
        {
            r.DeviceId = deviceId;
        }

        _context.Records.AddRange(list);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<Device>> GetAllDevicesAsync(CancellationToken ct = default)
        => await _context.Devices
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .ToListAsync(ct);

    public async Task<Device?> GetDeviceWithRecordsAsync(int id, CancellationToken ct = default)
        => await _context.Devices
            .Include(d => d.Records)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<int> CountRecordsAsync(int deviceId, CancellationToken ct = default)
        => await _context.Records.CountAsync(r => r.DeviceId == deviceId, ct);

    public async Task<List<DeviceSummary>> GetSummariesAsync(CancellationToken ct = default)
        => await _context.Devices
            .OrderBy(d => d.Id)
            .Select(d => new DeviceSummary(d.Id, d.Name, d.Records.Count))
            .ToListAsync(ct);

    public async Task<bool> RenameDeviceAsync(int id, string newName, CancellationToken ct = default)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (device is null) return false;

        device.Name = newName;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteDeviceAsync(int id, CancellationToken ct = default)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (device is null) return false;

        // покази видаляються каскадом
        _context.Devices.Remove(device);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SourceLoudShare> GetLoudShareBySourceAsync(string source, double threshold, CancellationToken ct = default)
    {
        var bySource = _context.Records.Where(r => r.Source == source);

        var total = await bySource.CountAsync(ct);
        var loud = await bySource.CountAsync(r => r.Value > threshold, ct);

        double? percent = total == 0 ? null : 100.0 * loud / total;
        return new SourceLoudShare(source, total, loud, percent);
    }

    public async Task<Device?> GetDeviceWithLoudRecordsAsync(int deviceId, string source, double threshold, CancellationToken ct = default)
        => await _context.Devices
            .Include(d => d.Records
                .Where(r => r.Source == source && r.Value > threshold)
                .OrderByDescending(r => r.Value))
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deviceId, ct);
}
