namespace UniverseDemo.Core
{
    /// <summary>
    /// Kích hoạt "Vũ trụ trong vũ trụ" (Fractal Universe).
    /// Một module chứa một SubRegistry bên trong nó để tự định tuyến lệnh.
    /// </summary>
    public interface INestedModule : IModule
    {
        ModuleRegistry SubRegistry { get; }
    }
}
