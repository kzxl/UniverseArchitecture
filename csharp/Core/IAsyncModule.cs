using System.Threading;
using System.Threading.Tasks;

namespace UniverseDemo.Core
{
    /// <summary>
    /// Contract cho module hỗ trợ xử lý Async/Await.
    /// Cho phép streaming, long-polling, và tác vụ I/O hiệu quả.
    /// </summary>
    public interface IAsyncModule : IModule
    {
        Task<string> ExecuteAsync(string command, string[] args, CancellationToken cancellationToken = default);
    }
}
