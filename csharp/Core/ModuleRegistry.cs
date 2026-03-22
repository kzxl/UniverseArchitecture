namespace UniverseDemo.Core;

/// <summary>
/// Registry — Trung tâm đăng ký module.
/// Module tự đăng ký, Core không biết chi tiết module.
/// Giống như vũ trụ tự tổ chức — thiên hà hình thành tự nhiên,
/// không cần "ai đó" hard-code danh sách thiên hà.
/// </summary>
public sealed class ModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Đăng ký 1 module. Trùng tên → exception.</summary>
    public void Register(IModule module)
    {
        if (!_modules.TryAdd(module.Name, module))
            throw new InvalidOperationException($"Module '{module.Name}' already registered.");
    }

    /// <summary>Dispatch command tới module phù hợp.</summary>
    public string Dispatch(string moduleName, string command, string[] args)
    {
        if (!_modules.TryGetValue(moduleName, out var module))
            throw new KeyNotFoundException($"Module '{moduleName}' not found. Available: {string.Join(", ", _modules.Keys)}");

        return module.Execute(command, args);
    }

    /// <summary>Lấy danh sách tất cả modules đã đăng ký.</summary>
    public IReadOnlyDictionary<string, IModule> GetAll() => _modules;

    /// <summary>Số lượng modules.</summary>
    public int Count => _modules.Count;
}
