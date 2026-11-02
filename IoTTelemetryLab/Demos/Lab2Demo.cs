using IoTTelemetryLab.Data;
using IoTTelemetryLab.Evaluation;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Monitoring;
using IoTTelemetryLab.Monitoring.Subscribers;
using IoTTelemetryLab.Processing;

namespace IoTTelemetryLab.Demos;

public static class Lab2Demo
{
    private const string DeviceId = "sensor-07";

    public static void Run()
    {
        // Завдання А (варіант 5): новий тип датчика реєструється ззовні, без правок існуючих класів
        var registry = EvaluatorSetup.CreateDefaultRegistry();
        registry.Register(SensorType.RotationSpeed, new RotationSpeedEvaluator());

        const SensorType sensorType = SensorType.RotationSpeed;
        var monitor = new TelemetryMonitor(registry, sensorType);

        var counter = new CriticalCounterSubscriber();
        var hourly = new HourlyCriticalGroupingSubscriber();

        // Multicast: три незалежних підписники на одну подію
        monitor.OnStatusEvaluated += ConsoleAlertSubscriber.OnStatusEvaluated;
        monitor.OnStatusEvaluated += counter.OnStatusEvaluated;
        monitor.OnStatusEvaluated += hourly.OnStatusEvaluated;

        // Діапазон 200–3500 RPM перекриває обидві зони Critical (< 500, > 3000) і обидві зони Warning
        var rawData = TelemetryGenerator.GenerateRawBatch(DeviceId, count: 50, minValue: 200, maxValue: 3500);
        var cleaned = TelemetryCleaner.RemoveInvalid(rawData).ToList();

        foreach (var reading in cleaned)
        {
            monitor.Process(reading);
        }

        Console.WriteLine($"\nВсього критичних станів зафіксовано лічильником: {counter.CriticalCount}");
        Console.WriteLine("Те саме постфактум через TelemetryAnalytics.CountByStatus: " +
            TelemetryAnalytics.CountByStatus(cleaned, sensorType, SafetyStatus.Critical, registry));
        PrintHourlyGroups(hourly);

        // Завдання А: перевірка порогів на підібраних вручну (граничних) значеннях
        double[] probes = { 0, 499.99, 500, 799.99, 800, 1500, 2500, 2500.01, 3000, 3000.01, 5000 };

        Console.WriteLine("\n=== Завдання А: RotationSpeedEvaluator на граничних значеннях ===");
        foreach (var value in probes)
        {
            Console.WriteLine($"{value,8} RPM -> {registry.Evaluate(sensorType, value)}");
        }

        // Завдання Б, п. 2: той самий оцінювач, але лямбдою через Func<double, SafetyStatus>
        var funcRegistry = EvaluatorSetup.CreateDefaultRegistry();
        funcRegistry.Register(SensorType.RotationSpeed, value => value switch
        {
            < 500 or > 3000 => SafetyStatus.Critical,
            < 800 or > 2500 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        });

        var allValues = cleaned.Select(r => r.Value).Concat(probes).ToList();
        int mismatches = allValues.Count(v =>
            registry.Evaluate(sensorType, v) != funcRegistry.Evaluate(sensorType, v));

        Console.WriteLine("\n=== Завдання Б: клас vs Func ===");
        Console.WriteLine($"Перевірено значень: {allValues.Count}, розбіжностей: {mismatches}");
        Console.WriteLine(mismatches == 0
            ? "Обидва оцінювачі дають однакові результати і взаємозамінні в реєстрі"
            : "УВАГА: оцінювачі дають різні результати!");

        // Завдання Б, пп. 3–4: згенерований пакет займає ~4 хв і майже завжди потрапляє в одну годину,
        // тому групування показуємо на ручному наборі показів у різні години доби.
        // Монітор працює з funcRegistry — підписнику байдуже, як задано стратегію
        var funcMonitor = new TelemetryMonitor(funcRegistry, sensorType);
        var hourlyDemo = new HourlyCriticalGroupingSubscriber();
        funcMonitor.OnStatusEvaluated += hourlyDemo.OnStatusEvaluated;

        var day = DateTime.UtcNow.Date;
        TelemetryReading[] manual =
        {
            new(DeviceId, 3200, day.AddHours(8).AddMinutes(5)),   // Critical
            new(DeviceId, 1500, day.AddHours(8).AddMinutes(20)),  // Normal
            new(DeviceId, 450, day.AddHours(8).AddMinutes(40)),   // Critical
            new(DeviceId, 2700, day.AddHours(14).AddMinutes(10)), // Warning
            new(DeviceId, 120, day.AddHours(14).AddMinutes(30)),  // Critical
            new(DeviceId, 3050, day.AddHours(22).AddMinutes(15)), // Critical
            new(DeviceId, 3000, day.AddHours(22).AddMinutes(45)), // Warning (межа)
        };

        foreach (var reading in manual)
        {
            funcMonitor.Process(reading);
        }

        Console.WriteLine("\n=== Завдання Б: групування critical-показів за годиною (ручний набір) ===");
        PrintHourlyGroups(hourlyDemo);
    }

    private static void PrintHourlyGroups(HourlyCriticalGroupingSubscriber subscriber)
    {
        Console.WriteLine("Critical-покази за годиною доби (UTC):");
        foreach (var (hour, readings) in subscriber.Groups)
        {
            Console.WriteLine($"  {hour:D2}:00–{hour:D2}:59 — {readings.Count} шт.: " +
                string.Join(", ", readings.Select(r => $"{r.Value} о {r.Timestamp:HH:mm:ss}")));
        }
    }
}
