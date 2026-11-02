using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

// Колишня гілка SoundLevel зі StatusEvaluator (ЛР1, варіант 5):
// понад 100 дБ шкодить слуху вже за ~15 хв на добу — Critical;
// понад 85 дБ — перевищено межу безпечного 8-годинного впливу, Warning
public class SoundLevelEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        > 100 => SafetyStatus.Critical,
        > 85 and <= 100 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}
