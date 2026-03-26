using UniverseDemo.Core;

namespace UniverseDemo.Modules.Notifier;

// ── Events ──

/// <summary>Event khi một phép tính được thực hiện.</summary>
public sealed class CalculationPerformedEvent
{
    public required string Operation { get; init; }
    public required string Result { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Event khi có lời chào.</summary>
public sealed class GreetingEvent
{
    public required string Name { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// NotifierModule — Module lắng nghe events từ modules khác.
/// Demo Principle #5: Indirect Communication via EventBus.
/// Không import Calculator hay Greeter — chỉ subscribe events.
/// </summary>
public sealed class NotifierModule : IModule, IModuleLifecycle
{
    private readonly IEventBus _eventBus;
    private readonly List<string> _notifications = [];
    private bool _initialized;

    public NotifierModule(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public string Name => "notifier";
    public string Description => "🔔 Listens to events from other modules";
    public IReadOnlyList<string> Commands => ["history", "count", "clear"];

    public string Execute(string command, string[] args) => command.ToLower() switch
    {
        "history" => _notifications.Count == 0
            ? "📭 No notifications yet."
            : string.Join("\n", _notifications.TakeLast(10).Select((n, i) => $"  {i + 1}. {n}")),
        "count" => $"📬 {_notifications.Count} notification(s)",
        "clear" => ClearNotifications(),
        _ => $"❓ Unknown command: {command}"
    };

    // ── Lifecycle ──

    public Task OnInitializing() => Task.CompletedTask;

    public Task OnInitialized()
    {
        // Subscribe events SAU khi đã đăng ký
        _eventBus.Subscribe<CalculationPerformedEvent>(OnCalculation);
        _eventBus.Subscribe<GreetingEvent>(OnGreeting);
        _initialized = true;
        return Task.CompletedTask;
    }

    public Task OnShuttingDown()
    {
        _eventBus.Unsubscribe<CalculationPerformedEvent>(OnCalculation);
        _eventBus.Unsubscribe<GreetingEvent>(OnGreeting);
        return Task.CompletedTask;
    }

    public Task OnShutdown()
    {
        _initialized = false;
        return Task.CompletedTask;
    }

    // ── Event Handlers ──

    private void OnCalculation(CalculationPerformedEvent e)
    {
        _notifications.Add($"🧮 Calculation: {e.Operation} = {e.Result}");
    }

    private void OnGreeting(GreetingEvent e)
    {
        _notifications.Add($"👋 Greeting: {e.Message}");
    }

    private string ClearNotifications()
    {
        var count = _notifications.Count;
        _notifications.Clear();
        return $"🗑️ Cleared {count} notification(s)";
    }
}
