using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Tasks.Domain;

/// <summary>
/// The whole lifecycle: an open task can be edited and done, a done one only reopened. Editing a
/// done task would rewrite what somebody already ticked off - reopen it first, which says so.
/// </summary>
public static class TaskItemStatusPolicy
{
    public const string AlreadyDoneMessage = "This task is already done.";

    public const string NotDoneMessage = "Only a done task can be reopened.";

    public const string DoneCannotChangeMessage = "A done task cannot be changed - reopen it first.";

    public static void EnsureCanComplete(TaskItem task)
    {
        if (task.Status == TaskItemStatus.Done)
            throw new BusinessRuleException(AlreadyDoneMessage);
    }

    public static void EnsureCanReopen(TaskItem task)
    {
        if (task.Status != TaskItemStatus.Done)
            throw new BusinessRuleException(NotDoneMessage);
    }

    public static void EnsureCanChange(TaskItem task)
    {
        if (task.Status == TaskItemStatus.Done)
            throw new BusinessRuleException(DoneCannotChangeMessage);
    }
}
