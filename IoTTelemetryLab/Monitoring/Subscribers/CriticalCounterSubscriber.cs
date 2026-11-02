using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Monitoring.Subscribers;

/// <summary>
/// Екземплярний обробник із внутрішнім станом: рахує критичні покази.
/// </summary>
public class CriticalCounterSubscriber
{
    public int CriticalCount { get; private set; }

    public void OnStatusEvaluated(TelemetryReading reading, SafetyStatus status)
    {
        if (status == SafetyStatus.Critical)
        {
            CriticalCount++;
        }
    }
}
