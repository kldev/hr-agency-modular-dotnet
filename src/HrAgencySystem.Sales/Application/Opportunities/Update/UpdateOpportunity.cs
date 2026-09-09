using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Application.Opportunities.Update;

public sealed record UpdateOpportunity(   
    Guid SalesOpportunityId,
    Guid OrganizationId,
    string Title,
    string Description,
    decimal ExpectedValue,
    CurrencyCode Currency,
    DateTimeOffset? ExpectedCloseDate,
    Guid ModifiedBy): IOpportunityData, IUpdateCommand;