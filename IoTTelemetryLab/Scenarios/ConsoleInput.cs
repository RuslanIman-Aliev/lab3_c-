using System.Globalization;

namespace IoTTelemetryLab.Scenarios;

public static class ConsoleInput
{
    public static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static bool TryReadDouble(string prompt, out double value)
    {
        var text = ReadLine(prompt).Replace(',', '.');

        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            Console.WriteLine("  Потрібно ввести число (наприклад, 25.5).");
            return false;
        }

        return true;
    }

    public static bool TryReadDate(string prompt, out DateTime value)
    {
        var text = ReadLine(prompt);
        string[] formats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd" };

        if (!DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
        {
            Console.WriteLine("  Невірний формат дати. Приклад: 2026-10-19 14:30");
            return false;
        }

        return true;
    }

    public static bool Confirm(string prompt)
    {
        return ReadLine(prompt).ToLowerInvariant() == "y";
    }
}
