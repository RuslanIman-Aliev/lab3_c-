using System.Text;
using IoTTelemetryLab.Data;
using Microsoft.EntityFrameworkCore;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

const int ReadingCount = 50;
const double MinValue = 30;
const double MaxValue = 120;

using var context = new AppDbContext();
ITelemetryRepository repository = new TelemetryRepository(context);

await context.Database.MigrateAsync();

// === Наповнення (лише якщо база порожня) ===
if ((await repository.GetAllDevicesAsync()).Count == 0)
{
    foreach (var name in new[] { "sensor-07", "sensor-12", "sensor-19" })
    {
        var saved = await DeviceSeeder.SeedAsync(repository, name, ReadingCount, MinValue, MaxValue);
        Console.WriteLine($"Створено '{name}'. Згенеровано: {ReadingCount}, збережено: {saved}");
    }
}

// === Демонстрація 1: N+1 ===
Console.WriteLine("\n--- N+1 ---");
SqlCommandCounter.Reset();
var devices = await repository.GetAllDevicesAsync();
foreach (var d in devices)
{
    var count = await repository.CountRecordsAsync(d.Id);
    Console.WriteLine($"  {d.Name}: {count}");
}
Console.WriteLine($"  SQL-команд: {SqlCommandCounter.Count}");

// === Демонстрація 2: проєкція ===
Console.WriteLine("\n--- Проєкція ---");
SqlCommandCounter.Reset();
var summaries = await repository.GetSummariesAsync();
foreach (var s in summaries)
{
    Console.WriteLine($"  {s.Name}: {s.RecordsCount}");
}
Console.WriteLine($"  SQL-команд: {SqlCommandCounter.Count}");

// === Демонстрація 3: Include ===
Console.WriteLine("\n--- Include ---");
SqlCommandCounter.Reset();
var withRecords = await repository.GetDeviceWithRecordsAsync(summaries[0].Id);
Console.WriteLine($"  '{withRecords?.Name}': показів {withRecords?.Records.Count}, SQL-команд: {SqlCommandCounter.Count}");

// === Демонстрація 4: CRUD на тимчасовому пристрої ===
Console.WriteLine("\n--- CRUD ---");
var temp = await repository.AddDeviceAsync("temp-sensor");
await repository.RenameDeviceAsync(temp.Id, "temp-sensor-renamed");
Console.WriteLine($"  Видалено: {await repository.DeleteDeviceAsync(temp.Id)}");
