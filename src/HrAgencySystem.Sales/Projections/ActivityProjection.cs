using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Projections;

public sealed record ActivityProjection(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid OrgId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid OpportunityId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    SalesActivityType ActivityType,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Note,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset CreatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CreatedById,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CompanyId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CompanySnapshot Company
)
{
    public static ActivityProjection Create(ActivityCreated @event)
    {
        return new ActivityProjection(
            @event.SalesActivityId,
            @event.OrganizationId,
            @event.SalesOpportunityId,
            @event.ActivityType,
            @event.Note,
            @event.CreatedAt,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.Company.Id,
            @event.Company);
    }
}