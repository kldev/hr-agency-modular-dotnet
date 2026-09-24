using HrAgencySystem.Sales.Domain.Activity;

namespace HrAgencySystem.Sales.Events.Opportunity;

/// <summary>
/// An activity was logged against the opportunity. The activity itself lives on its own stream;
/// this copy on the opportunity's stream is what lets <c>OpportunityProjection</c> say when the deal
/// was last worked on without reading another stream - the follow-up events do the same.
/// </summary>
public sealed record OpportunityActivityLogged(
    Guid OpportunityId,
    Guid OrganizationId,
    Guid ActivityId,
    SalesActivityType ActivityType,
    DateTimeOffset LoggedAt
);
