using IoTTelemetryLab.Data;

namespace IoTTelemetryLab.Scenarios;

public static class ReadScenario
{
    public static void Run()
    {
        using var context = new AppDbContext();

        var devices = context.Devices.OrderBy(d => d.Id).ToList();

        if (devices.Count == 0)
        {
            Console.WriteLine("У базі ще немає жодного сенсора. Спочатку наповніть базу (Крок 5).");
            return;
        }

        Console.WriteLine("=== Сенсори в базі даних ===");
        foreach (var device in devices)
        {
            var count = context.Records.Count(r => r.DeviceId == device.Id);
            Console.WriteLine($"  [{device.Id}] {device.Name} (показів: {count})");
        }

        while (true)
        {
            var input = ConsoleInput.ReadLine("\nВведіть Id сенсора (порожній рядок - вихід): ");

            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            if (!int.TryParse(input, out var id))
            {
                Console.WriteLine("  Потрібно ввести число - Id зі списку вище.");
                continue;
            }

            var selected = context.Devices.FirstOrDefault(d => d.Id == id);
            if (selected is null)
            {
                Console.WriteLine($"  Сенсора з Id={id} у базі немає.");
                continue;
            }

            var readings = context.Records
                .Where(r => r.DeviceId == selected.Id)
                .OrderBy(r => r.Timestamp)
                .ToList();

            Console.WriteLine($"\n--- Покази сенсора '{selected.Name}' ---");

            if (readings.Count == 0)
            {
                Console.WriteLine("  Показів немає.");
            }
            else
            {
                for (var i = 0; i < readings.Count; i++)
                {
                    var r = readings[i];
                    Console.WriteLine($"  {i + 1,3}. {r.Timestamp:yyyy-MM-dd HH:mm:ss}  {r.Value,8:F2} дБ  {r.Source}");
                }
            }

            Console.WriteLine($"Усього показів: {readings.Count}");
        }
    }
}
