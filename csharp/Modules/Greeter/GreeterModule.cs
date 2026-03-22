using UniverseDemo.Core;

namespace UniverseDemo.Modules.Greeter;

/// <summary>
/// Greeter Module — Module thứ 2 trong vũ trụ.
/// Chứng minh thêm module mới = thêm 1 class, register 1 dòng.
/// Không sửa code ở Core hay module khác.
/// </summary>
public class GreeterModule : IModule
{
    public string Name => "greeter";
    public string Description => "Greeting messages (hello, goodbye)";
    public IReadOnlyList<string> Commands => ["hello", "goodbye"];

    public string Execute(string command, string[] args)
    {
        var name = args.Length > 0 ? string.Join(" ", args) : "World";

        return command.ToLower() switch
        {
            "hello"   => $"👋 Hello, {name}! Welcome to the Universe!",
            "goodbye" => $"🌙 Goodbye, {name}! See you among the stars!",
            _ => $"Unknown command '{command}'. Available: {string.Join(", ", Commands)}"
        };
    }
}
