using IoTTelemetryLab.Data;

namespace IoTTelemetryLab.Scenarios;

public static class DeleteDeviceScenario
{
    public static void Run()
    {
        using var context = new AppDbContext();

        while (true)
        {
            var devices = context.Devices.OrderBy(d => d.Id).ToList();

            if (devices.Count == 0)
            {
                Console.WriteLine("У базі немає жодного сенсора.");
                return;
            }

            Console.WriteLine("\n=== Сенсори в базі даних ===");
            foreach (var d in devices)
            {
                var count = context.Records.Count(r => r.DeviceId == d.Id);
                Console.WriteLine($"  [{d.Id}] {d.Name} (показів: {count})");
            }

            var idInput = ConsoleInput.ReadLine("\nВведіть Id сенсора для видалення (порожній рядок - вихід): ");

            if (string.IsNullOrEmpty(idInput))
            {
                break;
            }

            if (!int.TryParse(idInput, out var id))
            {
                Console.WriteLine("  Потрібно ввести число - Id зі списку вище.");
                continue;
            }

            var device = context.Devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
            {
                Console.WriteLine($"  Сенсора з Id={id} у базі немає.");
                continue;
            }

            var recordsCount = context.Records.Count(r => r.DeviceId == id);
            Console.WriteLine($"  Буде видалено сенсор '{device.Name}' та {recordsCount} пов'язаних показів.");

            if (!ConsoleInput.Confirm("  Підтвердити видалення? (y/n): "))
            {
                Console.WriteLine("  Видалення скасовано.");
                continue;
            }

            var totalBefore = context.Records.Count();
            var deviceName = device.Name;

            context.Devices.Remove(device);
            context.SaveChanges();

            var totalAfter = context.Records.Count();
            var ownRecordsLeft = context.Records.Count(r => r.DeviceId == id);
            var orphans = context.Records.Count(r => !context.Devices.Any(d => d.Id == r.DeviceId));

            Console.WriteLine($"\n  Сенсор '{deviceName}' видалено.");
            Console.WriteLine($"  Усього показів у базі: було {totalBefore}, стало {totalAfter} (видалено {totalBefore - totalAfter}).");
            Console.WriteLine($"  Показів видаленого сенсора лишилось: {ownRecordsLeft}.");
            Console.WriteLine($"  Записів без батьківського сенсора (сиріт): {orphans}.");

            Console.WriteLine(ownRecordsLeft == 0 && orphans == 0
                ? "  Цілісність бази збережено."
                : "  УВАГА: виявлено порушення цілісності!");
        }
    }
}
