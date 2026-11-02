using System.Globalization;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Scenarios;

public static class DeleteRecordsScenario
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

            var scopeInput = ConsoleInput.ReadLine("\nId сенсора (0 - усі сенсори, порожній рядок - вихід): ");

            if (string.IsNullOrEmpty(scopeInput))
            {
                break;
            }

            if (!int.TryParse(scopeInput, out var scopeId))
            {
                Console.WriteLine("  Потрібно ввести число.");
                continue;
            }

            IQueryable<TelemetryRecord> query = context.Records;
            var scopeTitle = "усіх сенсорів";

            if (scopeId != 0)
            {
                var selected = devices.FirstOrDefault(d => d.Id == scopeId);
                if (selected is null)
                {
                    Console.WriteLine($"  Сенсора з Id={scopeId} у базі немає.");
                    continue;
                }

                query = query.Where(r => r.DeviceId == scopeId);
                scopeTitle = $"сенсора '{selected.Name}'";
            }

            Console.WriteLine("\nУмова видалення показів:");
            Console.WriteLine("  1 - значення нижче порога");
            Console.WriteLine("  2 - значення вище порога");
            Console.WriteLine("  3 - значення в діапазоні [від; до]");
            Console.WriteLine("  4 - покази, зафіксовані раніше за вказану дату");
            var choice = ConsoleInput.ReadLine("Оберіть умову (1-4): ");

            string conditionTitle;

            switch (choice)
            {
                case "1":
                    if (!ConsoleInput.TryReadDouble("  Поріг (видалити покази < поріг): ", out var below)) continue;
                    query = query.Where(r => r.Value < below);
                    conditionTitle = $"значення < {below.ToString(CultureInfo.InvariantCulture)}";
                    break;

                case "2":
                    if (!ConsoleInput.TryReadDouble("  Поріг (видалити покази > поріг): ", out var above)) continue;
                    query = query.Where(r => r.Value > above);
                    conditionTitle = $"значення > {above.ToString(CultureInfo.InvariantCulture)}";
                    break;

                case "3":
                    if (!ConsoleInput.TryReadDouble("  Нижня межа діапазону: ", out var min)) continue;
                    if (!ConsoleInput.TryReadDouble("  Верхня межа діапазону: ", out var max)) continue;
                    if (min > max)
                    {
                        Console.WriteLine("  Нижня межа не може бути більшою за верхню.");
                        continue;
                    }
                    query = query.Where(r => r.Value >= min && r.Value <= max);
                    conditionTitle = $"значення від {min.ToString(CultureInfo.InvariantCulture)} до {max.ToString(CultureInfo.InvariantCulture)}";
                    break;

                case "4":
                    if (!ConsoleInput.TryReadDate("  Дата (yyyy-MM-dd або yyyy-MM-dd HH:mm, UTC): ", out var before)) continue;
                    query = query.Where(r => r.Timestamp < before);
                    conditionTitle = $"зафіксовані раніше {before:yyyy-MM-dd HH:mm}";
                    break;

                default:
                    Console.WriteLine("  Невідома умова.");
                    continue;
            }

            var matchCount = query.Count();

            if (matchCount == 0)
            {
                Console.WriteLine("\n  Покази за цією умовою не знайдено.");
                continue;
            }

            Console.WriteLine($"\n  Знайдено показів: {matchCount} ({scopeTitle}; {conditionTitle})");

            var preview = query.OrderBy(r => r.Timestamp).Take(5).ToList();
            foreach (var r in preview)
            {
                Console.WriteLine($"    Id={r.Id}, сенсор={r.DeviceId}, {r.Timestamp:yyyy-MM-dd HH:mm:ss}, значення={r.Value:F2}");
            }

            if (matchCount > preview.Count)
            {
                Console.WriteLine($"    ... та ще {matchCount - preview.Count}");
            }

            if (!ConsoleInput.Confirm($"\n  Видалити {matchCount} показів? (y/n): "))
            {
                Console.WriteLine("  Видалення скасовано.");
                continue;
            }

            var totalBefore = context.Records.Count();

            var toDelete = query.ToList();
            context.Records.RemoveRange(toDelete);
            context.SaveChanges();

            var totalAfter = context.Records.Count();
            var stillMatching = query.Count();

            Console.WriteLine($"\n  Видалено показів: {totalBefore - totalAfter}.");
            Console.WriteLine($"  Усього показів у базі: було {totalBefore}, стало {totalAfter}.");
            Console.WriteLine($"  Показів, що ще підпадають під умову: {stillMatching}.");
        }
    }
}
