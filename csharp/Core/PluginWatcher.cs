using System;
using System.IO;
using System.Threading;

namespace UniverseDemo.Core
{
    /// <summary>
    /// FileSystem Watcher — Tự động phát hiện DLL mới trong thư mục plugins.
    /// Khi DLL xuất hiện → LoadPlugin(). Khi DLL bị xóa → UnloadPlugin().
    /// </summary>
    public sealed class PluginWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly ModuleRegistry _registry;
        private readonly string _pluginsDir;
        private Timer? _debounceTimer;

        public event Action<string>? OnPluginLoaded;
        public event Action<string>? OnPluginUnloaded;

        public PluginWatcher(ModuleRegistry registry, string pluginsDirectory)
        {
            _registry = registry;
            _pluginsDir = Path.GetFullPath(pluginsDirectory);

            if (!Directory.Exists(_pluginsDir))
                Directory.CreateDirectory(_pluginsDir);

            _watcher = new FileSystemWatcher(_pluginsDir, "*.dll")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = false
            };

            _watcher.Created += OnFileCreated;
            _watcher.Deleted += OnFileDeleted;
        }

        /// <summary>Bắt đầu theo dõi thư mục plugins.</summary>
        public void Start()
        {
            // Scan các DLL mới có sẵn trong folder
            foreach (var dll in Directory.GetFiles(_pluginsDir, "*.dll"))
            {
                TryLoadPlugin(dll);
            }

            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>Dừng theo dõi.</summary>
        public void Stop() => _watcher.EnableRaisingEvents = false;

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Debounce 500ms để đợi file copy xong
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ => TryLoadPlugin(e.FullPath), null, 500, Timeout.Infinite);
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            var pluginName = Path.GetFileNameWithoutExtension(e.FullPath);
            try
            {
                _registry.UnloadPlugin(pluginName);
                OnPluginUnloaded?.Invoke(pluginName);
            }
            catch { /* Plugin may not have been loaded */ }
        }

        private void TryLoadPlugin(string dllPath)
        {
            try
            {
                _registry.LoadPlugin(dllPath);
                var pluginName = Path.GetFileNameWithoutExtension(dllPath);
                OnPluginLoaded?.Invoke(pluginName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginWatcher] Failed to load {dllPath}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _debounceTimer?.Dispose();
            _watcher.Dispose();
        }
    }
}
