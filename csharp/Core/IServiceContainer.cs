namespace UniverseDemo.Core;

/// <summary>
/// Service lifetime — Vòng đời của service trong container.
/// </summary>
public enum Lifetime
{
    /// <summary>Mỗi lần Resolve tạo instance mới.</summary>
    Transient,
    /// <summary>Tạo 1 lần, dùng mãi.</summary>
    Singleton
}

/// <summary>
/// Không-thời gian (Spacetime) — DI Container nhẹ cho modules.
/// Modules resolve dependencies qua container thay vì tự tạo.
/// Principle #4: Registry — Not Hard-coded.
/// </summary>
public interface IServiceContainer
{
    /// <summary>Đăng ký service mapping.</summary>
    void Register<TInterface, TImpl>(Lifetime lifetime = Lifetime.Transient)
        where TInterface : class
        where TImpl : class, TInterface;

    /// <summary>Đăng ký instance cụ thể (singleton).</summary>
    void RegisterInstance<TInterface>(TInterface instance) where TInterface : class;

    /// <summary>Resolve service.</summary>
    T Resolve<T>() where T : class;

    /// <summary>Thử resolve, trả null nếu không tìm thấy.</summary>
    T? TryResolve<T>() where T : class;
}

/// <summary>
/// ServiceContainer — Lightweight DI container.
/// Không thay thế Autofac/MS DI mà là demo pattern.
/// </summary>
public sealed class ServiceContainer : IServiceContainer
{
    private readonly Dictionary<Type, ServiceDescriptor> _descriptors = [];

    public void Register<TInterface, TImpl>(Lifetime lifetime = Lifetime.Transient)
        where TInterface : class
        where TImpl : class, TInterface
    {
        _descriptors[typeof(TInterface)] = new ServiceDescriptor(typeof(TImpl), lifetime, null);
    }

    public void RegisterInstance<TInterface>(TInterface instance) where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        _descriptors[typeof(TInterface)] = new ServiceDescriptor(typeof(TInterface), Lifetime.Singleton, instance);
    }

    public T Resolve<T>() where T : class
    {
        return TryResolve<T>()
            ?? throw new InvalidOperationException($"Service '{typeof(T).Name}' not registered.");
    }

    public T? TryResolve<T>() where T : class
    {
        if (!_descriptors.TryGetValue(typeof(T), out var descriptor))
            return null;

        if (descriptor.Instance is T cached)
            return cached;

        var instance = (T)Activator.CreateInstance(descriptor.ImplType)!;
        if (descriptor.Lifetime == Lifetime.Singleton)
            descriptor.Instance = instance;

        return instance;
    }

    /// <summary>Số services đã đăng ký.</summary>
    public int Count => _descriptors.Count;

    private sealed class ServiceDescriptor(Type implType, Lifetime lifetime, object? instance)
    {
        public Type ImplType { get; } = implType;
        public Lifetime Lifetime { get; } = lifetime;
        public object? Instance { get; set; } = instance;
    }
}
