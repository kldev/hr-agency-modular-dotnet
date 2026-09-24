namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// The sale this delivery came from, with its title as it was when the link was made. Optional:
/// a project can be set up for a client before - or without - a deal being recorded.
/// </summary>
public sealed record ProjectOpportunity(Guid Id, string Title);
