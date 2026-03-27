using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniverseDemo.Core;

namespace UniverseDemo.Modules.Order
{
    /// <summary>
    /// Demo Workflow: Đặt hàng → Thanh toán → Giao hàng.
    /// Nếu fail ở bất kỳ bước nào, tự động rollback ngược lại.
    /// </summary>
    public class OrderWorkflow : IWorkflowModule
    {
        public string Name => "order-workflow";
        public string Description => "📦 Order processing workflow (Saga pattern)";
        public IReadOnlyList<string> Commands => new[] { "run", "status" };

        public string Execute(string command, string[] args)
        {
            return command switch
            {
                "run" => "Use WorkflowEngine.RunAsync() to execute this workflow.",
                "status" => "Use WorkflowEngine to check state.",
                _ => $"[order-workflow] Unknown: {command}"
            };
        }

        public IReadOnlyList<WorkflowStep> DefineSteps()
        {
            return new[]
            {
                new WorkflowStep
                {
                    Name = "CreateOrder",
                    Execute = async payload =>
                    {
                        // Giả lập tạo đơn hàng
                        payload["orderId"] = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
                        await Task.Delay(100);
                        return true;
                    },
                    Compensate = async payload =>
                    {
                        Console.WriteLine($"  ↩️  Cancelling order {payload["orderId"]}");
                        await Task.Delay(50);
                    }
                },
                new WorkflowStep
                {
                    Name = "ChargePayment",
                    Execute = async payload =>
                    {
                        // Giả lập thanh toán
                        payload["paymentId"] = $"PAY-{Guid.NewGuid().ToString()[..8]}";
                        await Task.Delay(200);
                        return true;
                    },
                    Compensate = async payload =>
                    {
                        Console.WriteLine($"  ↩️  Refunding payment {payload["paymentId"]}");
                        await Task.Delay(50);
                    }
                },
                new WorkflowStep
                {
                    Name = "ShipItem",
                    Execute = async payload =>
                    {
                        // Giả lập giao hàng
                        payload["trackingNo"] = $"SHIP-{new Random().Next(10000, 99999)}";
                        await Task.Delay(150);
                        return true;
                    },
                    Compensate = async payload =>
                    {
                        Console.WriteLine($"  ↩️  Recalling shipment {payload["trackingNo"]}");
                        await Task.Delay(50);
                    }
                }
            };
        }
    }
}
