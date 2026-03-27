using System.Collections.Generic;
using System.Threading.Tasks;
using UniverseDemo.Core;

namespace SamplePlugin
{
    /// <summary>
    /// 🔌 Plugin demo — được compile thành DLL riêng biệt.
    /// Khi drop vào thư mục /plugins, ModuleRegistry tự nạp và sử dụng ngay.
    /// </summary>
    public class WeatherModule : IModule, IModuleLifecycle
    {
        public string Name => "weather";
        public string Description => "🌤️ Weather forecast plugin (hot-loaded)";
        public IReadOnlyList<string> Commands => new[] { "today", "forecast" };

        public string Execute(string command, string[] args)
        {
            return command switch
            {
                "today" => "☀️ Today: 28°C, Sunny in Ho Chi Minh City",
                "forecast" => "🌧️ Tomorrow: 25°C, Rain expected. Pack an umbrella!",
                _ => $"[weather] Unknown command: {command}"
            };
        }

        public Task OnInitializing()
        {
            Console.WriteLine("  🔌 [WeatherPlugin] Initializing...");
            return Task.CompletedTask;
        }

        public Task OnInitialized()
        {
            Console.WriteLine("  ✅ [WeatherPlugin] Ready to serve forecasts!");
            return Task.CompletedTask;
        }

        public Task OnShuttingDown()
        {
            Console.WriteLine("  ⏳ [WeatherPlugin] Shutting down...");
            return Task.CompletedTask;
        }

        public Task OnShutdown()
        {
            Console.WriteLine("  ❌ [WeatherPlugin] Unloaded. Goodbye!");
            return Task.CompletedTask;
        }
    }
}
