using IoTTelemetryLab.Data;

namespace IoTTelemetryLab.Scenarios;

public static class UpdateScenario
{
    public static void Run()
    {
        using var context = new AppDbContext();

        while (true)
        {
            var devices = context.Devices.OrderBy(d => d.Id).ToList();

            if (devices.Count == 0)
            {
                Console.WriteLine("У базі ще немає жодного сенсора. Спочатку наповніть базу (Крок 5).");
                return;
            }

            Console.WriteLine("\n=== Сенсори в базі даних ===");
            foreach (var d in devices)
            {
                Console.WriteLine($"  [{d.Id}] {d.Name}");
            }

            var idInput = ConsoleInput.ReadLine("\nВведіть Id сенсора для перейменування (порожній рядок - вихід): ");

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

            Console.WriteLine($"  Поточна назва: '{device.Name}'");
            var newName = ConsoleInput.ReadLine("  Нова назва (порожній рядок - скасувати): ");

            if (string.IsNullOrEmpty(newName))
            {
                Console.WriteLine("  Перейменування скасовано.");
                continue;
            }

            if (newName.Length > LabSettings.MaxNameLength)
            {
                Console.WriteLine($"  Назва задовга: максимум {LabSettings.MaxNameLength} символів.");
                continue;
            }

            if (newName == device.Name)
            {
                Console.WriteLine("  Нова назва збігається з поточною, змін немає.");
                continue;
            }

            var nameTaken = context.Devices.Any(d => d.Name == newName && d.Id != id);
            if (nameTaken)
            {
                Console.WriteLine($"  Сенсор з назвою '{newName}' уже існує.");
                continue;
            }

            var oldName = device.Name;
            device.Name = newName;
            context.SaveChanges();

            Console.WriteLine($"  Готово: '{oldName}' -> '{device.Name}'");
        }
    }
}
