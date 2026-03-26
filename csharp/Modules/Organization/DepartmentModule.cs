using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniverseDemo.Core;

namespace UniverseDemo.Modules.Organization
{
    /// <summary>
    /// VÍ DỤ 1: Nested Module (Fractal Universe)
    /// Đóng vai trò như một Sub-Registry quản lý các nhân sự.
    /// </summary>
    public class DepartmentModule : INestedModule
    {
        public string Name => "hr";
        public string Description => "Human Resources Department (Nested Registry)";
        public IReadOnlyList<string> Commands => new[] { "list" };

        public ModuleRegistry SubRegistry { get; } = new();

        public DepartmentModule()
        {
            // Hr chứa 2 nhân sự (ngôi sao nhỏ)
            SubRegistry.Register(new EmployeeModule("john"));
            SubRegistry.Register(new EmployeeModule("jane"));
            
            // Nested middleware — Áp dụng local cho hr thôi
            SubRegistry.AddMiddleware(new LocalHrAuthMiddleware());
        }

        public string Execute(string command, string[] args)
        {
            if (command == "list")
                return $"HR department has {SubRegistry.Count} employees.";
            return $"[Department] Unknown command: {command}";
        }
    }

    /// <summary>
    /// Employee nhận lệnh từ trong HR. Vừa hỗ trợ Async, vừa bảo mật cấp cao.
    /// </summary>
    public class EmployeeModule : IAsyncModule, ISecureModule
    {
        public string Name { get; }
        public string Description => $"Employee details for {Name}";
        public IReadOnlyList<string> Commands => new[] { "salary", "ping_async" };

        public EmployeeModule(string name)
        {
            Name = name;
        }

        // VÍ DỤ 2: Module Sandboxing & Permissions
        public ModulePermissionPolicy GetPolicy() => new ModulePermissionPolicy
        {
            RequiredClaims = new[] { "admin" } // Yêu cầu người gọi phải có quyền `admin` để check lương
        };

        public string Execute(string command, string[] args)
        {
            if (command == "salary")
                return $"[CONFIDENTIAL] {Name}'s salary is $1000.";
            return $"[{Name}] Unknown command: {command}";
        }

        // VÍ DỤ 3: Async Execution
        public async Task<string> ExecuteAsync(string command, string[] args, CancellationToken cancellationToken = default)
        {
            if (command == "ping_async")
            {
                await Task.Delay(500, cancellationToken); // Fake I/O or HTTP call
                return $"[{Name}] Pong async! Waited 500ms.";
            }
            
            return Execute(command, args);
        }
    }

    /// <summary> Local middleware cho nested registry </summary>
    public class LocalHrAuthMiddleware : IMiddleware
    {
        public Task InvokeAsync(ModuleContext context, Func<Task> next)
        {
            Console.WriteLine($"      [Sub-Gravity] HR-Auth intercepting: {context.ModuleName}");
            return next();
        }
    }
}
