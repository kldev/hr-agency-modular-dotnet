using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Domain.ValueObjects;

namespace HrAgencySystem.Tasks.Application;

/// <summary>What the task drawer sends, for creating and changing a task alike.</summary>
public interface ITaskItemInput
{
    string Title { get; }
    string? Description { get; }
    DateTimeOffset DueAt { get; }
    TaskPriority Priority { get; }
}

public sealed record ValidTaskItemInput(string Title, string? Description, DateTimeOffset DueAt, TaskPriority Priority);

/// <summary>Every field problem at once, in one <see cref="ValidationException"/>.</summary>
public static class TaskItemInputValidator
{
    public const int DescriptionMaxLength = 2000;

    public const string DescriptionTooLongMessage = "A description cannot exceed 2000 characters.";

    public const string DueAtRequiredMessage = "A due date is required.";

    public const string UnknownPriorityMessage = "Unknown priority.";

    public static ValidTaskItemInput Validate(ITaskItemInput input)
    {
        var errors = new List<string>();

        var (title, titleError) = TaskTitle.TryCreate(input.Title);
        if (titleError is not null)
            errors.Add(titleError);

        var description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        if (description is { Length: > DescriptionMaxLength })
            errors.Add(DescriptionTooLongMessage);

        if (input.DueAt == default)
            errors.Add(DueAtRequiredMessage);

        if (!Enum.IsDefined(input.Priority))
            errors.Add(UnknownPriorityMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new ValidTaskItemInput(title!.Value, description, input.DueAt.ToUniversalTime(), input.Priority);
    }
}
