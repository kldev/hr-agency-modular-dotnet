
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Sales.Application.Activity.Create;

public sealed record CreateSalesActivity(
    Guid OrganizationId,
    Guid SalesOpportunityId,
    SalesActivityType ActivityType,
    string Note,
    Guid CreatedBy):ICreateCommand;