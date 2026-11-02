using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Demos;
using IoTTelemetryLab.Scenarios;
using Microsoft.EntityFrameworkCore;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

using (var context = new AppDbContext())
{
    context.Database.Migrate();
}

while (true)
{
    Console.WriteLine("\n==================================================");
    Console.WriteLine(" IoTTelemetryLab — ЛР3, варіант 5 (рівень шуму, дБ)");
    Console.WriteLine("==================================================");
    Console.WriteLine("  1 - Наповнення бази (Крок 5, Create)");
    Console.WriteLine("  2 - Перегляд сенсорів і показів (Крок 6, Read)");
    Console.WriteLine("  3 - Перейменування сенсора (Крок 7.1, Update)");
    Console.WriteLine("  4 - Видалення сенсора (Крок 7.2, Delete + каскад)");
    Console.WriteLine("  5 - Видалення показів за умовою (Крок 7.3, Delete)");
    Console.WriteLine($"  6 - Індивідуальний запит: % показів > {LabSettings.LoudThreshold} дБ за джерелом (Крок 10)");
    Console.WriteLine("  7 - Демонстрація ЛР1/ЛР2 (без бази даних)");
    Console.WriteLine("  0 - Вихід");

    var choice = ConsoleInput.ReadLine("Оберіть пункт: ");
    Console.WriteLine();

    switch (choice)
    {
        case "1": SeedScenario.Run(); break;
        case "2": ReadScenario.Run(); break;
        case "3": UpdateScenario.Run(); break;
        case "4": DeleteDeviceScenario.Run(); break;
        case "5": DeleteRecordsScenario.Run(); break;
        case "6": SourceQueryScenario.Run(); break;
        case "7": Lab2Demo.Run(); break;
        case "0":
        case "":
            return;
        default:
            Console.WriteLine("Невідомий пункт меню.");
            break;
    }
}
