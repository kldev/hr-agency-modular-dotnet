using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Documents;

public sealed record FollowUpAction(
    Guid Id,
    Guid OpportunityId,
    Guid OrganizationId,
    string Content,
    DateTimeOffset FollowDateTime,
    DateTimeOffset CreatedAt,
    CompanySnapshot Company,
    UserSnapshot CreatedBy);
    