using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace UniverseDemo.Core.Infrastructure
{
    /// <summary>
    /// VÍ DỤ 4: Distributed Event Bus Adapter
    /// Đây là nền móng để chứng minh Universe Architecture có thể migrate
    /// từ In-Process (Memory) sang Distributed (RabbitMQ/Kafka) mà KHÔNG thay đổi
    /// bất kỳ dòng code business nào của Module.
    /// </summary>
    public class RabbitMqEventBusAdapter : IEventBus
    {
        // Giả lập connection string
        private readonly string _connectionString;

        public RabbitMqEventBusAdapter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Publish<T>(T @event) where T : class
        {
            var topic = typeof(T).Name;
            var payload = JsonSerializer.Serialize(@event);
            
            // FIXME: Thực tế sẽ gọi RabbitMQ channel.BasicPublish(...)
            Console.WriteLine($"[RabbitMQ-Stub] Broadcasting to exchange '{topic}': {payload}");
        }

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            var topic = typeof(T).Name;
            
            // FIXME: Thực tế sẽ tạo queue binding tới exchange '{topic}'
            Console.WriteLine($"[RabbitMQ-Stub] Subscribing to queue for '{topic}'");
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            var topic = typeof(T).Name;
            Console.WriteLine($"[RabbitMQ-Stub] Unsubscribing from '{topic}'");
        }
    }
}
