using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Application.Opportunities.Create;

public sealed record CreateOpportunity(
    Guid OrganizationId,
    Guid CompanyId,
    string Title,
    string Description,
    decimal ExpectedValue,
    bool IsHotLead,
    CurrencyCode Currency,
    DateTimeOffset? ExpectedCloseDate,
    Guid? ResponsibleId,
    Guid CreatedBy) : IOpportunityData, ICreateCommand;
