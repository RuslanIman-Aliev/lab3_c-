using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

/// <summary>
/// Стратегія оцінки показу: перетворює значення на рівень небезпеки.
/// </summary>
public interface ISensorEvaluator
{
    SafetyStatus Evaluate(double value);
}
