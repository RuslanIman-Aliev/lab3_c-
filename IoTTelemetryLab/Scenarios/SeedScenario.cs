using IoTTelemetryLab.Data;

namespace IoTTelemetryLab.Scenarios;

public static class SeedScenario
{
    public static void Run()
    {
        using var context = new AppDbContext();

        Console.WriteLine("=== Наповнення бази телеметрією ===");
        Console.WriteLine("Введіть назву сенсора (порожній рядок - завершити введення).\n");

        while (true)
        {
            var name = ConsoleInput.ReadLine("Назва сенсора: ");

            if (string.IsNullOrEmpty(name))
            {
                break;
            }

            if (name.Length > LabSettings.MaxNameLength)
            {
                Console.WriteLine($"  Назва задовга: максимум {LabSettings.MaxNameLength} символів.\n");
                continue;
            }

            if (context.Devices.Any(d => d.Name == name))
            {
                Console.WriteLine($"  Сенсор '{name}' уже є в базі, пропускаємо.\n");
                continue;
            }

            DeviceSeeder.SeedDevice(context, name, LabSettings.ReadingCount,
                LabSettings.MinValue, LabSettings.MaxValue);

            var savedCount = context.Records.Count(r => r.Device.Name == name);
            Console.WriteLine($"  Сенсор '{name}' створено, додано показів до БД: {savedCount}.\n");
        }

        var devices = context.Devices.OrderBy(d => d.Id).ToList();
        Console.WriteLine($"\nУсього сенсорів у базі: {devices.Count}");

        foreach (var device in devices)
        {
            var recordsCount = context.Records.Count(r => r.DeviceId == device.Id);
            Console.WriteLine($"  Id={device.Id}, Name={device.Name}, показів: {recordsCount}");
        }
    }
}
