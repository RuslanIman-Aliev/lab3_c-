using IoTTelemetryLab.Evaluation;
using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

/// <summary>
/// Аналітичні LINQ-запити над очищеними показами.
/// </summary>
public static class TelemetryAnalytics
{
    /// <summary>
    /// Підраховує кількість показів із заданим рівнем небезпеки.
    /// </summary>
    /// <param name="readings">Очищені покази.</param>
    /// <param name="type">Тип датчика (визначає пороги).</param>
    /// <param name="target">Рівень небезпеки, що підраховується.</param>
    /// <param name="registry">Реєстр стратегій оцінки.</param>
    /// <returns>Кількість показів із рівнем <paramref name="target"/>.</returns>
    public static int CountByStatus(
        IEnumerable<TelemetryReading> readings,
        SensorType type,
        SafetyStatus target,
        SensorEvaluatorRegistry registry)
    {
        return readings.Count(r => registry.Evaluate(type, r.Value) == target);
    }

    // Ковзне середнє за вікном N показів — типова аналітична задача для телеметрії
    /// <summary>
    /// Обчислює ковзне середнє значень за вікном фіксованого розміру.
    /// </summary>
    /// <param name="readings">Очищені покази.</param>
    /// <param name="windowSize">Розмір вікна.</param>
    /// <returns>Послідовність із (кількість показів − windowSize + 1) середніх.</returns>
    public static IEnumerable<double> MovingAverage(
        IEnumerable<TelemetryReading> readings, int windowSize)
    {
        var values = readings.Select(r => r.Value).ToList();

        for (int i = 0; i <= values.Count - windowSize; i++)
        {
            yield return values.Skip(i).Take(windowSize).Average();
        }
    }

    // Варіант 5: знаходить час (Timestamp) першого Critical-показу в потоці.
    // LINQ: Where — відбір лише критичних показів через StatusEvaluator,
    // Select — проєкція на DateTime? (щоб відсутність результату давала null, а не DateTime.MinValue),
    // FirstOrDefault — перший елемент або null, якщо критичних показів немає.
    /// <summary>
    /// Повертає час першого показу зі статусом <see cref="SafetyStatus.Critical"/>.
    /// </summary>
    /// <param name="readings">Очищені покази, впорядковані за часом.</param>
    /// <param name="type">Тип датчика (визначає пороги).</param>
    /// <param name="registry">Реєстр стратегій оцінки.</param>
    /// <returns>Timestamp першого критичного показу або <c>null</c>, якщо таких немає.</returns>
    public static DateTime? FirstCriticalTimestamp(
        IEnumerable<TelemetryReading> readings, SensorType type, SensorEvaluatorRegistry registry)
    {
        return readings
            .Where(r => registry.Evaluate(type, r.Value) == SafetyStatus.Critical)
            .Select(r => (DateTime?)r.Timestamp)
            .FirstOrDefault();
    }
}
