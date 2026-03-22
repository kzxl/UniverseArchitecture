namespace UniverseDemo.Shared;

/// <summary>
/// Shared Infrastructure — "Không-thời gian" mà modules sống trên.
/// Cung cấp utilities dùng chung, không chứa business logic.
/// </summary>
public static class ConsoleHelper
{
    public static void PrintHeader(string title)
    {
        var line = new string('═', 60);
        Console.WriteLine();
        Console.WriteLine(line);
        Console.WriteLine($"  🌌 {title}");
        Console.WriteLine(line);
    }

    public static void PrintResult(string module, string command, string[] args, string result)
    {
        Console.WriteLine($"  ▸ {module} {command} {string.Join(" ", args)}");
        Console.WriteLine($"    → {result}");
    }

    public static void PrintBenchmark(string label, long iterations, double elapsedMs)
    {
        var opsPerSec = iterations / (elapsedMs / 1000.0);
        Console.WriteLine($"  ⚡ {label}: {iterations:N0} ops in {elapsedMs:F1}ms ({opsPerSec:N0} ops/sec)");
    }
}
