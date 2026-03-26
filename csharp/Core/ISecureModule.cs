using System;
using System.Collections.Generic;

namespace UniverseDemo.Core
{
    /// <summary>
    /// Chính sách bảo mật và quyền hạn của Module.
    /// Ngăn chặn module truy cập trái phép Middleware hoặc EventBus.
    /// </summary>
    public class ModulePermissionPolicy
    {
        public IReadOnlyList<string> RequiredClaims { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> AllowedSubscribes { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> AllowedPublishes { get; init; } = Array.Empty<string>();
        
        /// <summary>
        /// Default open policy for backward compatibility.
        /// </summary>
        public static readonly ModulePermissionPolicy Open = new ModulePermissionPolicy
        {
            AllowedSubscribes = new[] { "*" },
            AllowedPublishes = new[] { "*" }
        };
    }

    /// <summary>
    /// Module có thể bắt buộc phải có Policy này.
    /// Nếu không implement, tự động dùng Open policy.
    /// </summary>
    public interface ISecureModule : IModule
    {
        ModulePermissionPolicy GetPolicy();
    }
    
    public class ModulePermissionException : Exception
    {
        public ModulePermissionException(string module, string action, string resource)
            : base($"Module '{module}' không có quyền {action} trên resource '{resource}'.")
        {
        }
    }
}
