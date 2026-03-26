namespace UniverseDemo.Core;

/// <summary>
/// ModuleContext — Ngữ cảnh cho mỗi lần dispatch, đi qua middleware pipeline.
/// Chứa thông tin input/output + metadata.
/// </summary>
public sealed class ModuleContext
{
    /// <summary>Tên module đích.</summary>
    public required string ModuleName { get; init; }

    /// <summary>Command cần thực thi.</summary>
    public required string Command { get; init; }

    /// <summary>Tham số.</summary>
    public required string[] Args { get; init; }

    /// <summary>Kết quả sau khi execute.</summary>
    public string? Result { get; set; }

    /// <summary>Metadata key-value (middleware có thể gắn thêm data).</summary>
    public Dictionary<string, object> Items { get; } = [];

    /// <summary>Timestamp khi context được tạo.</summary>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Lực hấp dẫn (Gravity) — Middleware tự động apply lên mọi dispatch.
/// Principle #7: Middleware = Gravity.
/// Chain of Responsibility pattern.
/// </summary>
public interface IMiddleware
{
    /// <summary>
    /// Xử lý context, gọi next() để chuyển tiếp xuống middleware kế tiếp.
    /// Không gọi next() = short-circuit (chặn pipeline).
    /// </summary>
    Task InvokeAsync(ModuleContext context, Func<Task> next);
}
