using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

// ЛР2, варіант 5 (Завдання А): швидкість обертання, RPM.
// Critical: < 500 or > 3000; Warning: < 800 or > 2500
public class RotationSpeedEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        < 500 or > 3000 => SafetyStatus.Critical,
        < 800 or > 2500 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}
