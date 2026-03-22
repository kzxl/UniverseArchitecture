using UniverseDemo.Core;

namespace UniverseDemo.Modules.Calculator;

/// <summary>
/// Calculator Module — Một "ngôi sao" trong vũ trụ.
/// Tự chứa, độc lập, tuân thủ IModule interface.
/// Xóa folder này → app vẫn build (Acceptance Test #1).
/// </summary>
public class CalculatorModule : IModule
{
    public string Name => "calculator";
    public string Description => "Basic arithmetic operations (add, sub, mul, div)";
    public IReadOnlyList<string> Commands => ["add", "sub", "mul", "div"];

    public string Execute(string command, string[] args)
    {
        if (args.Length < 2)
            return "Error: need 2 numbers. Usage: calculator add 1 2";

        if (!double.TryParse(args[0], out var a) || !double.TryParse(args[1], out var b))
            return $"Error: invalid numbers '{args[0]}' '{args[1]}'";

        return command.ToLower() switch
        {
            "add" => $"{a} + {b} = {a + b}",
            "sub" => $"{a} - {b} = {a - b}",
            "mul" => $"{a} * {b} = {a * b}",
            "div" => b == 0 ? "Error: division by zero" : $"{a} / {b} = {a / b}",
            _ => $"Unknown command '{command}'. Available: {string.Join(", ", Commands)}"
        };
    }
}
