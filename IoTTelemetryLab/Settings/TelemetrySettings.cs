namespace IoTTelemetryLab.Settings;

public class TelemetrySettings
{
    public int ReadingCount { get; set; } = 50;
    public double MinValue { get; set; } = 30;
    public double MaxValue { get; set; } = 120;
    public double LoudThreshold { get; set; } = 85;
}
