using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

/// <summary>
/// Відповідність «тип датчика → стратегія оцінки» замість switch по SensorType.
/// </summary>
public class SensorEvaluatorRegistry
{
    private readonly Dictionary<SensorType, ISensorEvaluator> _evaluators = new();

    public void Register(SensorType type, ISensorEvaluator evaluator)
    {
        _evaluators[type] = evaluator;
    }

    // Завдання Б: оцінювач можна задати лямбдою або посиланням на метод
    public void Register(SensorType type, Func<double, SafetyStatus> evaluatorFunc)
    {
        Register(type, new FuncSensorEvaluator(evaluatorFunc));
    }

    public SafetyStatus Evaluate(SensorType type, double value)
    {
        if (!_evaluators.TryGetValue(type, out var evaluator))
        {
            throw new InvalidOperationException(
                $"Немає зареєстрованого оцінювача для типу {type}");
        }

        return evaluator.Evaluate(value);
    }
}
