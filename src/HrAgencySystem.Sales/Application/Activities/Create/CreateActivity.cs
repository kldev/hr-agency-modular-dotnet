using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Sales.Application.Activities.Create;

public sealed record CreateActivity(
    Guid OrganizationId,
    Guid SalesOpportunityId,
    SalesActivityType ActivityType,
    string Note,
    Guid CreatedBy):ICreateCommand;