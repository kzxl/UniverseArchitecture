namespace UniverseDemo.Core;

/// <summary>
/// Registry — Trung tâm đăng ký module.
/// Upgraded: Tích hợp EventBus, ServiceContainer, Middleware pipeline, Lifecycle hooks.
/// Giống vũ trụ tự tổ chức — thiên hà hình thành tự nhiên,
/// lực hấp dẫn (middleware) tác động mọi dispatch.
/// </summary>
public sealed class ModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IMiddleware> _middlewares = [];

    /// <summary>Event Bus — giao tiếp gián tiếp giữa modules.</summary>
    public EventBus EventBus { get; } = new();

    /// <summary>Service Container — DI cho modules.</summary>
    public ServiceContainer Services { get; } = new();

    /// <summary>Đăng ký 1 module. Trùng tên → exception. Hỗ trợ lifecycle hooks.</summary>
    public void Register(IModule module)
    {
        if (!_modules.TryAdd(module.Name, module))
            throw new InvalidOperationException($"Module '{module.Name}' already registered.");
    }

    /// <summary>Đăng ký module với lifecycle hooks (async).</summary>
    public async Task RegisterAsync(IModule module)
    {
        // OnInitializing — trước khi đăng ký
        if (module is IModuleLifecycle lifecycle)
            await lifecycle.OnInitializing();

        Register(module);

        // OnInitialized — sau khi đăng ký
        if (module is IModuleLifecycle lc)
            await lc.OnInitialized();
    }

    /// <summary>Thêm middleware vào pipeline. Order matters — FIFO.</summary>
    public void AddMiddleware(IMiddleware middleware) => _middlewares.Add(middleware);

    /// <summary>Dispatch command tới module, qua middleware pipeline.</summary>
    public string Dispatch(string moduleName, string command, string[] args)
    {
        if (!_modules.TryGetValue(moduleName, out var module))
            throw new KeyNotFoundException($"Module '{moduleName}' not found. Available: {string.Join(", ", _modules.Keys)}");

        if (_middlewares.Count == 0)
            return module.Execute(command, args);

        // Build context + run pipeline
        var context = new ModuleContext
        {
            ModuleName = moduleName,
            Command = command,
            Args = args
        };

        RunPipeline(context, module).GetAwaiter().GetResult();
        return context.Result ?? module.Execute(command, args);
    }

    /// <summary>Dispatch async qua middleware pipeline.</summary>
    public async Task<string> DispatchAsync(string moduleName, string command, string[] args)
    {
        if (!_modules.TryGetValue(moduleName, out var module))
            throw new KeyNotFoundException($"Module '{moduleName}' not found. Available: {string.Join(", ", _modules.Keys)}");

        if (_middlewares.Count == 0)
            return module.Execute(command, args);

        var context = new ModuleContext
        {
            ModuleName = moduleName,
            Command = command,
            Args = args
        };

        await RunPipeline(context, module);
        return context.Result ?? module.Execute(command, args);
    }

    /// <summary>Shutdown tất cả modules có lifecycle.</summary>
    public async Task ShutdownAsync()
    {
        foreach (var module in _modules.Values)
        {
            if (module is IModuleLifecycle lifecycle)
            {
                await lifecycle.OnShuttingDown();
                await lifecycle.OnShutdown();
            }
        }
    }

    private Task RunPipeline(ModuleContext context, IModule module)
    {
        var index = -1;

        Task Next()
        {
            index++;
            if (index < _middlewares.Count)
                return _middlewares[index].InvokeAsync(context, Next);

            // End of pipeline → execute module
            context.Result = module.Execute(context.Command, context.Args);
            return Task.CompletedTask;
        }

        return Next();
    }

    /// <summary>Lấy danh sách tất cả modules đã đăng ký.</summary>
    public IReadOnlyDictionary<string, IModule> GetAll() => _modules;

    /// <summary>Số lượng modules.</summary>
    public int Count => _modules.Count;

    /// <summary>Số lượng middlewares.</summary>
    public int MiddlewareCount => _middlewares.Count;
}
