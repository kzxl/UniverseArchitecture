namespace UniverseDemo.Core;

/// <summary>
/// Vòng đời của ngôi sao (Star Lifecycle) — Từ tinh vân → ngôi sao → siêu tân tinh.
/// Module có thể implement lifecycle hooks để khởi tạo/dọn dẹp tài nguyên.
/// Optional interface — module không implement thì bỏ qua.
/// </summary>
public interface IModuleLifecycle
{
    /// <summary>Gọi TRƯỚC khi module được đăng ký vào Registry.</summary>
    Task OnInitializing();

    /// <summary>Gọi SAU khi module đã đăng ký thành công.</summary>
    Task OnInitialized();

    /// <summary>Gọi TRƯỚC khi shutdown.</summary>
    Task OnShuttingDown();

    /// <summary>Gọi SAU khi đã shutdown hoàn toàn.</summary>
    Task OnShutdown();
}
