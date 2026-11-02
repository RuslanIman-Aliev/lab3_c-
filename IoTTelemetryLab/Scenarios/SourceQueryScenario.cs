using IoTTelemetryLab.Data;

namespace IoTTelemetryLab.Scenarios;

public static class SourceQueryScenario
{
    private const int SensorCount = 5;

    public static void Run()
    {
        using var context = new AppDbContext();

        Console.WriteLine("=== Наповнення бази ===");

        for (var i = 1; i <= SensorCount; i++)
        {
            var name = $"sensor-{i:D2}";

            if (context.Devices.Any(d => d.Name == name))
            {
                Console.WriteLine($"  '{name}' уже є в базі, пропускаємо.");
                continue;
            }

            DeviceSeeder.SeedDevice(context, name, LabSettings.ReadingCount,
                LabSettings.MinValue, LabSettings.MaxValue);

            var saved = context.Records.Count(r => r.Device.Name == name);
            Console.WriteLine($"  '{name}': згенеровано {LabSettings.ReadingCount}, збережено {saved}");
        }

        while (true)
        {
            Console.WriteLine($"\n=== Відсоток показів > {LabSettings.LoudThreshold} дБ за джерелом шуму ===");
            for (var i = 0; i < DeviceSeeder.Sources.Length; i++)
            {
                Console.WriteLine($"  {i + 1} - {DeviceSeeder.Sources[i]}");
            }

            var input = ConsoleInput.ReadLine("Номер або назва джерела (порожній рядок - вихід): ");

            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            if (!TryParseSource(input, out var source))
            {
                Console.WriteLine($"  Невідоме джерело '{input}'. Введіть номер 1-{DeviceSeeder.Sources.Length} або назву зі списку.");
                continue;
            }

            var bySource = context.Records.Where(r => r.Source == source);

            var total = bySource.Count();
            var loud = bySource.Count(r => r.Value > LabSettings.LoudThreshold);

            Console.WriteLine($"\n--- Джерело: {source} ---");

            if (total == 0)
            {
                Console.WriteLine("  Немає даних: для цього джерела в базі немає показів.");
                continue;
            }

            var percent = 100.0 * loud / total;

            Console.WriteLine($"  Усього показів:            {total,5}");
            Console.WriteLine($"  Показів > {LabSettings.LoudThreshold} дБ:          {loud,5}");
            Console.WriteLine($"  Частка гучних показів:     {percent,8:F2} %");
        }
    }

    private static bool TryParseSource(string input, out string source)
    {
        if (int.TryParse(input, out var number) && number >= 1 && number <= DeviceSeeder.Sources.Length)
        {
            source = DeviceSeeder.Sources[number - 1];
            return true;
        }

        var match = DeviceSeeder.Sources
            .FirstOrDefault(s => string.Equals(s, input, StringComparison.OrdinalIgnoreCase));

        source = match ?? string.Empty;
        return match is not null;
    }
}
