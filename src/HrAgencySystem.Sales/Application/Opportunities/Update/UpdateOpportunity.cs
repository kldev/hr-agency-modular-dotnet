using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.ValueObjects;
using JasperFx;

namespace HrAgencySystem.Sales.Application.Opportunities.Update;

public sealed record UpdateOpportunity(   
    [property: Identity] Guid OpportunityId,
    Guid OrganizationId,
    string Title,
    string Description,
    decimal ExpectedValue,
    bool IsHotLead,
    CurrencyCode Currency,
    DateTimeOffset? ExpectedCloseDate,
    Guid ModifiedBy): IOpportunityData, IUpdateCommand;