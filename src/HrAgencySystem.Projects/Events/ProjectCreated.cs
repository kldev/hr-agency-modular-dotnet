using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectCreated(
    Guid ProjectId,
    Guid OrganizationId,
    CompanySnapshot Company,
    DeliveringEntitySnapshot DeliveringEntity,
    string Name,
    string Description,
    EngagementType EngagementType,
    Placement Placement,
    Guid? TeamId,
    string? TeamName,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    ProjectOpportunity? Opportunity = null
);
