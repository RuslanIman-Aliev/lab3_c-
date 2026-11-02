using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

// Адаптер: робить з делегата Func<double, SafetyStatus> повноцінну стратегію ISensorEvaluator,
// тож реєстр зберігає однорідні екземпляри інтерфейсу незалежно від способу їх задання
internal sealed class FuncSensorEvaluator : ISensorEvaluator
{
    private readonly Func<double, SafetyStatus> _evaluate;

    public FuncSensorEvaluator(Func<double, SafetyStatus> evaluate)
    {
        _evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
    }

    public SafetyStatus Evaluate(double value) => _evaluate(value);
}
