namespace HrAgencySystem.Tasks.Domain;

/// <summary>The deal a task belongs to, with its title as it was when the task was saved.</summary>
public sealed record TaskOpportunity(Guid Id, string Title);
