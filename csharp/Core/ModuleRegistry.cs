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
        return DispatchAsync(moduleName, command, args).GetAwaiter().GetResult();
    }

    /// <summary>Dispatch async qua middleware pipeline. Hỗ trợ Nested Registry và Async Execution.</summary>
    public async Task<string> DispatchAsync(string moduleName, string command, string[] args)
    {
        // 1. Phân tích nested routing (Fractal Universe)
        var parts = moduleName.Split('.', 2);
        var rootModule = parts[0];
        
        if (!_modules.TryGetValue(rootModule, out var module))
            throw new KeyNotFoundException($"Module '{rootModule}' not found. Available: {string.Join(", ", _modules.Keys)}");

        // 1b. Gửi tiếp xuống SubRegistry nếu có nested route
        if (parts.Length > 1)
        {
            if (module is INestedModule nested)
                return await nested.SubRegistry.DispatchAsync(parts[1], command, args);
            throw new InvalidOperationException($"Module '{rootModule}' does not support nested routing (Not an INestedModule).");
        }

        // 2. Build context
        var context = new ModuleContext
        {
            ModuleName = moduleName,
            Command = command,
            Args = args
        };

        // 3. Chạy pipeline (Lực hấp dẫn) -> Execution
        await RunPipeline(context, module);
        return context.Result ?? await ExecuteModuleCore(module, command, args);
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

            // End of pipeline → Không execute CŨ ở đây nữa, đẩy về DispatchAsync chờ Result
            return Task.CompletedTask;
        }

        return Next();
    }

    private async Task<string> ExecuteModuleCore(IModule module, string command, string[] args)
    {
        // Ưu tiên Async Execution nếu module hỗ trợ
        if (module is IAsyncModule asyncModule)
        {
            return await asyncModule.ExecuteAsync(command, args);
        }
        
        // Fallback về sync execution
        return module.Execute(command, args);
    }

    /// <summary>Lấy danh sách tất cả modules đã đăng ký.</summary>
    public IReadOnlyDictionary<string, IModule> GetAll() => _modules;

    /// <summary>Số lượng modules.</summary>
    public int Count => _modules.Count;

    /// <summary>Số lượng middlewares.</summary>
    public int MiddlewareCount => _middlewares.Count;
}
