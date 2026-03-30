using Xunit;
using UniverseDemo.Core;
using UniverseDemo.Modules.Calculator;
using UniverseDemo.Modules.Greeter;

namespace UniverseDemo.Tests;

#region ModuleRegistry Tests

public class ModuleRegistryTests
{
    [Fact]
    public void Register_ValidModule_IsAccessible()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());

        Assert.Equal(1, registry.Count);
        Assert.True(registry.GetAll().ContainsKey("calculator"));
    }

    [Fact]
    public void Register_DuplicateName_ThrowsException()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register(new CalculatorModule()));
    }

    [Fact]
    public void Dispatch_ValidCommand_ReturnsCorrectResult()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());

        var result = registry.Dispatch("calculator", "add", ["10", "25"]);
        Assert.Equal("10 + 25 = 35", result);
    }

    [Fact]
    public void Dispatch_UnknownModule_ThrowsKeyNotFound()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());

        Assert.Throws<KeyNotFoundException>(() =>
            registry.Dispatch("nonexistent", "x", []));
    }

    [Fact]
    public void Dispatch_CaseInsensitive_Works()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());

        var result = registry.Dispatch("CALCULATOR", "add", ["1", "2"]);
        Assert.Equal("1 + 2 = 3", result);
    }

    [Fact]
    public void Count_ReflectsRegistrations()
    {
        var registry = new ModuleRegistry();
        Assert.Equal(0, registry.Count);

        registry.Register(new CalculatorModule());
        Assert.Equal(1, registry.Count);

        registry.Register(new GreeterModule());
        Assert.Equal(2, registry.Count);
    }
}

#endregion

#region Calculator Module Tests

public class CalculatorModuleTests
{
    private readonly CalculatorModule _calc = new();

    [Theory]
    [InlineData("add", "10", "25", "10 + 25 = 35")]
    [InlineData("sub", "100", "37", "100 - 37 = 63")]
    [InlineData("mul", "7", "8", "7 * 8 = 56")]
    [InlineData("div", "22", "7", "22 / 7 = 3.142857142857143")]
    public void Execute_ValidCommand_ReturnsCorrectResult(
        string command, string a, string b, string expected)
    {
        var result = _calc.Execute(command, [a, b]);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Execute_DivisionByZero_ReturnsError()
    {
        var result = _calc.Execute("div", ["10", "0"]);
        Assert.Equal("Error: division by zero", result);
    }

    [Fact]
    public void Execute_TooFewArgs_ReturnsError()
    {
        var result = _calc.Execute("add", ["5"]);
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void Execute_InvalidNumbers_ReturnsError()
    {
        var result = _calc.Execute("add", ["abc", "def"]);
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void Execute_UnknownCommand_ReturnsError()
    {
        var result = _calc.Execute("sqrt", ["9"]);
        Assert.Contains("Unknown command", result);
    }

    [Fact]
    public void Metadata_IsCorrect()
    {
        Assert.Equal("calculator", _calc.Name);
        Assert.NotEmpty(_calc.Description);
        Assert.Contains("add", _calc.Commands);
        Assert.Contains("sub", _calc.Commands);
        Assert.Contains("mul", _calc.Commands);
        Assert.Contains("div", _calc.Commands);
    }
}

#endregion

#region Greeter Module Tests

public class GreeterModuleTests
{
    private readonly GreeterModule _greeter = new();

    [Fact]
    public void Execute_Hello_ReturnsGreeting()
    {
        var result = _greeter.Execute("hello", ["Universe"]);
        Assert.Equal("👋 Hello, Universe! Welcome to the Universe!", result);
    }

    [Fact]
    public void Execute_Goodbye_ReturnsFarewell()
    {
        var result = _greeter.Execute("goodbye", ["Developer"]);
        Assert.Equal("🌙 Goodbye, Developer! See you among the stars!", result);
    }

    [Fact]
    public void Execute_NoArgs_DefaultsToWorld()
    {
        var result = _greeter.Execute("hello", []);
        Assert.Equal("👋 Hello, World! Welcome to the Universe!", result);
    }

    [Fact]
    public void Execute_MultiWordName_JoinsArgs()
    {
        var result = _greeter.Execute("hello", ["John", "Doe"]);
        Assert.Equal("👋 Hello, John Doe! Welcome to the Universe!", result);
    }

    [Fact]
    public void Execute_UnknownCommand_ReturnsError()
    {
        var result = _greeter.Execute("wave", []);
        Assert.Contains("Unknown command", result);
    }

    [Fact]
    public void Metadata_IsCorrect()
    {
        Assert.Equal("greeter", _greeter.Name);
        Assert.NotEmpty(_greeter.Description);
        Assert.Contains("hello", _greeter.Commands);
        Assert.Contains("goodbye", _greeter.Commands);
    }
}

#endregion

#region Acceptance Tests

public class AcceptanceTests
{
    /// <summary>
    /// Test 1: Module Isolation — tạo registry chỉ 1 module,
    /// module còn lại không ảnh hưởng.
    /// </summary>
    [Fact]
    public void AcceptanceTest1_ModuleIsolation_RegistryWorksWithPartialModules()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        // Không register Greeter — simulate "xóa module"

        // Calculator vẫn hoạt động bình thường
        var result = registry.Dispatch("calculator", "add", ["1", "2"]);
        Assert.Equal("1 + 2 = 3", result);

        // Greeter không tồn tại → exception nhưng không crash registry
        Assert.Throws<KeyNotFoundException>(() =>
            registry.Dispatch("greeter", "hello", ["World"]));

        // Calculator vẫn OK sau khi greeter fail
        var resultAfter = registry.Dispatch("calculator", "mul", ["3", "4"]);
        Assert.Equal("3 * 4 = 12", resultAfter);
    }

    /// <summary>
    /// Test 2: Zero Core Changes — thêm DummyModule mới mà không sửa Core.
    /// </summary>
    [Fact]
    public void AcceptanceTest2_ZeroCoreChanges_NewModuleWithoutModifyingCore()
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        registry.Register(new GreeterModule());
        registry.Register(new TestDummyModule("echo"));

        Assert.Equal(3, registry.Count);
        var result = registry.Dispatch("echo", "ping", []);
        Assert.Equal("pong", result);

        // Existing modules vẫn OK
        Assert.Equal("1 + 2 = 3",
            registry.Dispatch("calculator", "add", ["1", "2"]));
    }

    /// <summary>
    /// Test 3: Consistent Behavior — verify output format chuẩn.
    /// </summary>
    [Theory]
    [InlineData("calculator", "add", new[] { "10", "25" }, "10 + 25 = 35")]
    [InlineData("calculator", "sub", new[] { "100", "37" }, "100 - 37 = 63")]
    [InlineData("calculator", "mul", new[] { "7", "8" }, "7 * 8 = 56")]
    [InlineData("greeter", "hello", new[] { "Universe" }, "👋 Hello, Universe! Welcome to the Universe!")]
    [InlineData("greeter", "goodbye", new[] { "Developer" }, "🌙 Goodbye, Developer! See you among the stars!")]
    public void AcceptanceTest3_ConsistentBehavior_OutputMatchesSpec(
        string moduleName, string command, string[] args, string expected)
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        registry.Register(new GreeterModule());

        var result = registry.Dispatch(moduleName, command, args);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Test 4: Scalability — registry vẫn hoạt động với nhiều modules.
    /// </summary>
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void AcceptanceTest4_Scalability_RegistryHandlesManyModules(int count)
    {
        var registry = new ModuleRegistry();
        registry.Register(new CalculatorModule());
        for (int i = 1; i < count; i++)
            registry.Register(new TestDummyModule($"dummy_{i}"));

        Assert.Equal(count, registry.Count);

        // Module gốc vẫn dispatch đúng
        var result = registry.Dispatch("calculator", "add", ["5", "3"]);
        Assert.Equal("5 + 3 = 8", result);
    }
}

/// <summary>Helper module for acceptance tests — proves new module = just implement IModule.</summary>
internal sealed class TestDummyModule(string name) : IModule
{
    public string Name => name;
    public string Description => "Test dummy";
    public IReadOnlyList<string> Commands => ["ping"];
    public string Execute(string command, string[] args) => "pong";
}

#endregion
