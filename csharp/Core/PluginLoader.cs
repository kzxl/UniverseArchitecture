using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace UniverseDemo.Core
{
    /// <summary>
    /// Plugin Loader — Nạp/Gỡ module từ file DLL khi app đang chạy.
    /// Sử dụng AssemblyLoadContext để cô lập bộ nhớ (Isolated Universe).
    /// </summary>
    public sealed class PluginLoader
    {
        private readonly Dictionary<string, PluginContext> _loaded = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Nạp tất cả IModule từ DLL. Trả về danh sách modules đã tìm thấy.
        /// </summary>
        public IReadOnlyList<IModule> Load(string dllPath)
        {
            var fullPath = Path.GetFullPath(dllPath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Plugin DLL not found: {fullPath}");

            var pluginName = Path.GetFileNameWithoutExtension(fullPath);
            if (_loaded.ContainsKey(pluginName))
                throw new InvalidOperationException($"Plugin '{pluginName}' is already loaded.");

            // Tạo isolated context để có thể unload sau này
            var context = new PluginLoadContext(fullPath);
            var assembly = context.LoadFromAssemblyPath(fullPath);

            // Scan tất cả types implement IModule
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            var modules = new List<IModule>();
            foreach (var type in moduleTypes)
            {
                if (Activator.CreateInstance(type) is IModule module)
                    modules.Add(module);
            }

            _loaded[pluginName] = new PluginContext(context, modules);
            return modules;
        }

        /// <summary>
        /// Gỡ plugin đã nạp trước đó. Trả về danh sách module names bị gỡ.
        /// </summary>
        public IReadOnlyList<string> Unload(string pluginName)
        {
            if (!_loaded.TryGetValue(pluginName, out var ctx))
                throw new KeyNotFoundException($"Plugin '{pluginName}' is not loaded.");

            var names = ctx.Modules.Select(m => m.Name).ToList();

            // Giải phóng reference để GC có thể thu hồi
            ctx.Context.Unload();
            _loaded.Remove(pluginName);

            return names;
        }

        /// <summary>Danh sách plugin đang active.</summary>
        public IReadOnlyCollection<string> LoadedPlugins => _loaded.Keys;

        // ── Inner classes ──

        private sealed record PluginContext(AssemblyLoadContext Context, List<IModule> Modules);

        private sealed class PluginLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public PluginLoadContext(string pluginPath) : base(isCollectible: true)
            {
                _resolver = new AssemblyDependencyResolver(pluginPath);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                var path = _resolver.ResolveAssemblyToPath(assemblyName);
                return path != null ? LoadFromAssemblyPath(path) : null;
            }
        }
    }
}
