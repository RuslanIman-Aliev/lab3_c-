namespace IoTTelemetryLab.Models;

// Percent = null, якщо для джерела немає жодного показу
public record SourceLoudShare(string Source, int Total, int Loud, double? Percent);
