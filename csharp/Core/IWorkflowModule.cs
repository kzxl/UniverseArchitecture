using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UniverseDemo.Core
{
    /// <summary>
    /// Trạng thái của Workflow — được serialize/deserialize ra file để resume sau crash.
    /// </summary>
    public enum WorkflowStatus { Pending, Running, Completed, Failed, Compensating }

    /// <summary>
    /// Thông tin 1 bước trong quy trình.
    /// </summary>
    public sealed class WorkflowStep
    {
        public required string Name { get; init; }
        public required Func<Dictionary<string, object>, Task<bool>> Execute { get; init; }
        public Func<Dictionary<string, object>, Task>? Compensate { get; init; }
    }

    /// <summary>
    /// Contract cho module hỗ trợ Temporal Workflow (Saga Pattern).
    /// Module định nghĩa danh sách steps, Engine tự chạy tuần tự + persist state.
    /// </summary>
    public interface IWorkflowModule : IModule
    {
        IReadOnlyList<WorkflowStep> DefineSteps();
    }

    /// <summary>
    /// State lưu trữ tiến trình của workflow (persisted to disk).
    /// </summary>
    public sealed class WorkflowState
    {
        public string WorkflowName { get; set; } = "";
        public int CurrentStepIndex { get; set; }
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
        public Dictionary<string, object> Payload { get; set; } = new();
        public List<string> CompletedSteps { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}
