namespace IoTTelemetryLab.Models;

/// <summary>
/// Тип фізичної величини, яку вимірює датчик.
/// </summary>
public enum SensorType
{
    Temperature,
    Vibration,
    Voltage,
    SoundLevel,   // ЛР1, варіант 5: рівень шуму, дБ
    RotationSpeed // ЛР2, варіант 5: швидкість обертання, RPM
}
