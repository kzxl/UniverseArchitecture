namespace UniverseDemo.Core;

/// <summary>
/// Sóng hấp dẫn (Gravitational Waves) — Cơ chế giao tiếp gián tiếp giữa modules.
/// Module A publish event → Module B subscribe → Không import trực tiếp.
/// Principle #5: Indirect Communication.
/// </summary>
public interface IEventBus
{
    /// <summary>Publish event tới tất cả subscribers.</summary>
    void Publish<TEvent>(TEvent @event) where TEvent : class;

    /// <summary>Subscribe nhận event khi được publish.</summary>
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    /// <summary>Hủy subscription.</summary>
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
}

/// <summary>
/// EventBus implementation — Dictionary-based, type-safe, in-process.
/// Level 2 communication (In-Process Event Bus).
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];

    public void Publish<TEvent>(TEvent @event) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
            return;

        // Iterate copy to allow modification during iteration
        foreach (var handler in handlers.ToList())
            ((Action<TEvent>)handler)(@event);
    }

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            handlers = [];
            _handlers[typeof(TEvent)] = handlers;
        }
        handlers.Add(handler);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            handlers.Remove(handler);
    }

    /// <summary>Số lượng event types đang có subscribers.</summary>
    public int TypeCount => _handlers.Count;

    /// <summary>Tổng số handlers.</summary>
    public int HandlerCount => _handlers.Values.Sum(h => h.Count);
}
