using System;
using System.Linq;
using System.Threading.Tasks;

namespace UniverseDemo.Core
{
    /// <summary>
    /// Middleware xác thực quyền hạn (Sandboxing).
    /// Đảm bảo Caller có đủ Claims để truy cập vào ISecureModule.
    /// </summary>
    public class AccessControlMiddleware : IMiddleware
    {
        private readonly ModuleRegistry _registry;

        public AccessControlMiddleware(ModuleRegistry registry)
        {
            _registry = registry;
        }

        public async Task InvokeAsync(ModuleContext context, Func<Task> next)
        {
            var modules = _registry.GetAll();
            var targetModule = context.ModuleName.Split('.')[0]; // Handle nested routing
            
            if (modules.TryGetValue(targetModule, out var module) && module is ISecureModule secureModule)
            {
                var policy = secureModule.GetPolicy();
                
                // Kiểm tra required claims
                foreach (var claim in policy.RequiredClaims)
                {
                    if (!context.Claims.Contains(claim))
                    {
                        var msg = $"Access Denied: Caller lacks claim '{claim}' to execute {context.Command} on module '{context.ModuleName}'.";
                        context.Result = $"[403 FORBIDDEN] {msg}";
                        
                        // Short-circuit pipeline
                        return;
                    }
                }
            }

            // Mọi thứ OK, đi tiếp qua các hành tinh khác
            await next();
        }
    }
}
