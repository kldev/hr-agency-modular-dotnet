using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Events.Activity;

public sealed record SalesActivityCreated(
    Guid SalesActivityId,
    Guid OrganizationId,
    Guid SalesOpportunityId,
    SalesActivityType ActivityType,
    string Note,
    DateTimeOffset CreatedAt,
    UserSnapshot CreatedBy,
    CompanySnapshot Company);
