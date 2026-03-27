using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace UniverseDemo.Core
{
    /// <summary>
    /// Workflow Engine — Chạy steps tuần tự, persist checkpoint sau mỗi bước.
    /// Hỗ trợ Resume (tiếp tục từ bước bị crash) và Compensate (rollback ngược).
    /// </summary>
    public sealed class WorkflowEngine
    {
        private readonly string _stateDir;

        public WorkflowEngine(string stateDirectory = "workflow_state")
        {
            _stateDir = Path.GetFullPath(stateDirectory);
            if (!Directory.Exists(_stateDir))
                Directory.CreateDirectory(_stateDir);
        }

        /// <summary>
        /// Chạy workflow từ đầu hoặc resume từ checkpoint cũ.
        /// </summary>
        public async Task<WorkflowState> RunAsync(IWorkflowModule workflow)
        {
            var steps = workflow.DefineSteps();
            var state = LoadState(workflow.Name) ?? new WorkflowState
            {
                WorkflowName = workflow.Name,
                StartedAt = DateTimeOffset.UtcNow,
                Status = WorkflowStatus.Running
            };

            // Resume? Log it
            if (state.CurrentStepIndex > 0 && state.Status == WorkflowStatus.Running)
                Console.WriteLine($"  🔄 Resuming '{workflow.Name}' from step {state.CurrentStepIndex} ({steps[state.CurrentStepIndex].Name})");

            state.Status = WorkflowStatus.Running;

            for (int i = state.CurrentStepIndex; i < steps.Count; i++)
            {
                var step = steps[i];
                state.CurrentStepIndex = i;
                SaveState(state); // Checkpoint TRƯỚC khi chạy

                Console.WriteLine($"  ▶ Step {i + 1}/{steps.Count}: {step.Name}...");

                try
                {
                    var success = await step.Execute(state.Payload);
                    if (!success)
                    {
                        state.ErrorMessage = $"Step '{step.Name}' returned failure.";
                        state.Status = WorkflowStatus.Failed;
                        Console.WriteLine($"  ❌ Step '{step.Name}' failed. Starting compensation...");
                        await CompensateAsync(steps, state, i);
                        SaveState(state);
                        return state;
                    }

                    state.CompletedSteps.Add(step.Name);
                    Console.WriteLine($"  ✅ Step '{step.Name}' completed.");
                }
                catch (Exception ex)
                {
                    state.ErrorMessage = ex.Message;
                    state.Status = WorkflowStatus.Failed;
                    Console.WriteLine($"  💥 Step '{step.Name}' threw: {ex.Message}");
                    await CompensateAsync(steps, state, i);
                    SaveState(state);
                    return state;
                }
            }

            state.Status = WorkflowStatus.Completed;
            state.CompletedAt = DateTimeOffset.UtcNow;
            SaveState(state);
            Console.WriteLine($"  🎉 Workflow '{workflow.Name}' completed successfully!");
            return state;
        }

        /// <summary>Rollback ngược từ step hiện tại về step 0.</summary>
        private async Task CompensateAsync(IReadOnlyList<WorkflowStep> steps, WorkflowState state, int failedIndex)
        {
            state.Status = WorkflowStatus.Compensating;
            Console.WriteLine($"  ⏪ Compensating from step {failedIndex} backwards...");

            for (int i = failedIndex; i >= 0; i--)
            {
                var step = steps[i];
                if (step.Compensate != null)
                {
                    Console.WriteLine($"  ↩️  Compensating: {step.Name}...");
                    try
                    {
                        await step.Compensate(state.Payload);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ⚠️  Compensation for '{step.Name}' failed: {ex.Message}");
                    }
                }
            }
        }

        private WorkflowState? LoadState(string workflowName)
        {
            var path = Path.Combine(_stateDir, $"{workflowName}.json");
            if (!File.Exists(path)) return null;

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<WorkflowState>(json);
            }
            catch
            {
                return null;
            }
        }

        private void SaveState(WorkflowState state)
        {
            var path = Path.Combine(_stateDir, $"{state.WorkflowName}.json");
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        /// <summary>Xóa state file (reset workflow).</summary>
        public void ClearState(string workflowName)
        {
            var path = Path.Combine(_stateDir, $"{workflowName}.json");
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
