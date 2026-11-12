using TeamHub.Domain.Exceptions;

namespace TeamHub.Domain.WorkItems;

/// <summary>Nơi duy nhất định nghĩa chuyển trạng thái hợp lệ của task (cột trên Kanban board).</summary>
public static class WorkItemWorkflow
{
    private static readonly Dictionary<WorkItemStatus, WorkItemStatus[]> Transitions = new()
    {
        [WorkItemStatus.Todo] = [WorkItemStatus.InProgress],
        [WorkItemStatus.InProgress] = [WorkItemStatus.Todo, WorkItemStatus.InReview, WorkItemStatus.Testing],
        [WorkItemStatus.InReview] = [WorkItemStatus.InProgress, WorkItemStatus.Testing],
        [WorkItemStatus.Testing] = [WorkItemStatus.InProgress, WorkItemStatus.Done],
        [WorkItemStatus.Done] = [WorkItemStatus.InProgress] // mở lại task
    };

    public static bool CanTransition(WorkItemStatus from, WorkItemStatus to) =>
        Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static void EnsureCanTransition(WorkItemStatus from, WorkItemStatus to)
    {
        if (!CanTransition(from, to))
            throw new InvalidTransitionException("Task", from.ToString(), to.ToString());
    }
}
