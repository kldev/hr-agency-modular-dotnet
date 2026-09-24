namespace HrAgencySystem.Tasks.Domain;

/// <summary>
/// Two states on purpose. "In progress" would be a second flag somebody has to remember to move,
/// and a task that is being worked on is simply one that is not done yet.
/// </summary>
public enum TaskItemStatus
{
    Open,
    Done,
}
