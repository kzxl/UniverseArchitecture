using System.Diagnostics;

namespace UniverseDemo.Core.Middleware;

/// <summary>
/// Logging Middleware — Ghi log mọi dispatch (module, command, args, result).
/// Gravity tự động apply, module không cần biết.
/// </summary>
public sealed class LoggingMiddleware : IMiddleware
{
    private readonly List<string> _logs = [];

    public async Task InvokeAsync(ModuleContext context, Func<Task> next)
    {
        var log = $"[LOG] {context.Timestamp:HH:mm:ss.fff} → {context.ModuleName}.{context.Command}({string.Join(", ", context.Args)})";
        await next();
        log += $" → {context.Result}";
        _logs.Add(log);
    }

    /// <summary>Lấy tất cả logs đã ghi.</summary>
    public IReadOnlyList<string> Logs => _logs;
}

/// <summary>
/// Timing Middleware — Đo thời gian mỗi dispatch.
/// Gắn elapsed time vào context.Items["elapsed_ms"].
/// </summary>
public sealed class TimingMiddleware : IMiddleware
{
    public async Task InvokeAsync(ModuleContext context, Func<Task> next)
    {
        var sw = Stopwatch.StartNew();
        await next();
        sw.Stop();
        context.Items["elapsed_ms"] = sw.Elapsed.TotalMilliseconds;
    }
}

/// <summary>
/// Error Handling Middleware — Bắt exception, trả error message thay vì crash.
/// </summary>
public sealed class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(ModuleContext context, Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            context.Result = $"❌ Error: {ex.Message}";
        }
    }
}
