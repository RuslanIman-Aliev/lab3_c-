namespace IoTTelemetryLab.Models;

// readonly record struct: незмінний, легкий (value type) — ідеально
// для великої кількості короткоживучих показів з датчиків
/// <summary>
/// Незмінний показ IoT-датчика: ідентифікатор пристрою, виміряне значення та час вимірювання.
/// </summary>
/// <param name="DeviceId">Ідентифікатор пристрою, що надіслав показ.</param>
/// <param name="Value">Виміряне значення фізичної величини (може бути NaN або -999 у разі апаратної помилки).</param>
/// <param name="Timestamp">Момент вимірювання (UTC).</param>
public readonly record struct TelemetryReading(
    string DeviceId,
    double Value,
    DateTime Timestamp);
