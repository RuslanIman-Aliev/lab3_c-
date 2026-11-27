using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

// ===== ФАЗА 1. РЕЄСТРАЦІЯ СЛУЖБ =====

var builder = Host.CreateApplicationBuilder(args);

// шлях до файлу бази робимо абсолютним (відносно папки з .exe)
var csb = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Не знайдено ConnectionStrings:Default в appsettings.json"));
csb.DataSource = Path.Combine(AppContext.BaseDirectory, csb.DataSource);

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlite(csb.ConnectionString)
    .LogTo(SqlCommandCounter.OnLog,
           new[] { DbLoggerCategory.Database.Command.Name },
           LogLevel.Information));

builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();

builder.Services.Configure<TelemetrySettings>(builder.Configuration.GetSection("Telemetry"));

// ===== ФАЗА 2. ПОБУДОВА =====

using var host = builder.Build();

// ===== ФАЗА 3. ВИКОРИСТАННЯ =====

// у консольному застосунку scope створюємо вручну
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

await services.GetRequiredService<AppDbContext>().Database.MigrateAsync();

var repository = services.GetRequiredService<ITelemetryRepository>();
var settings = services.GetRequiredService<IOptions<TelemetrySettings>>().Value;

// === Наповнення (лише якщо база порожня) ===
if ((await repository.GetAllDevicesAsync()).Count == 0)
{
    foreach (var name in new[] { "sensor-07", "sensor-12", "sensor-19" })
    {
        var saved = await DeviceSeeder.SeedAsync(
            repository, name, settings.ReadingCount, settings.MinValue, settings.MaxValue);
        Console.WriteLine($"Створено '{name}'. Згенеровано: {settings.ReadingCount}, збережено: {saved}");
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

// === Варіант 5, завдання А: частка показів вище порога за джерелом ===
Console.WriteLine($"\n--- Варіант 5 (А): частка показів > {settings.LoudThreshold} дБ за джерелом ---");
foreach (var source in DeviceSeeder.Sources.Append("невідоме"))
{
    SqlCommandCounter.Reset();
    var share = await repository.GetLoudShareBySourceAsync(source, settings.LoudThreshold);

    var percentText = share.Percent is null ? "немає даних" : $"{share.Percent:F2} %";
    Console.WriteLine($"  {share.Source,-12} усього: {share.Total,3}, гучних: {share.Loud,3}, частка: {percentText}, SQL-команд: {SqlCommandCounter.Count}");
}

// === Варіант 5, завдання Б: пристрій з гучними показами джерела ===
const string LoudSource = "двигун";
Console.WriteLine($"\n--- Варіант 5 (Б): покази джерела '{LoudSource}' > {settings.LoudThreshold} дБ за спаданням ---");
SqlCommandCounter.Reset();
var loudDevice = await repository.GetDeviceWithLoudRecordsAsync(summaries[0].Id, LoudSource, settings.LoudThreshold);

if (loudDevice is null)
{
    Console.WriteLine("  Пристрій не знайдено.");
}
else
{
    Console.WriteLine($"  '{loudDevice.Name}': знайдено показів {loudDevice.Records.Count}, SQL-команд: {SqlCommandCounter.Count}");
    foreach (var r in loudDevice.Records)
    {
        Console.WriteLine($"    {r.Timestamp:yyyy-MM-dd HH:mm:ss}  {r.Value,7:F2} дБ  {r.Source}");
    }
}

// порожня вибірка: невідоме джерело та неіснуючий пристрій не дають винятку
var emptySource = await repository.GetDeviceWithLoudRecordsAsync(summaries[0].Id, "невідоме", settings.LoudThreshold);
var missingDevice = await repository.GetDeviceWithLoudRecordsAsync(-1, LoudSource, settings.LoudThreshold);
Console.WriteLine($"  Невідоме джерело: показів {emptySource?.Records.Count}; неіснуючий пристрій: {(missingDevice is null ? "null" : missingDevice.Name)}");
