using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Application.Opportunity.Create;

public sealed record CreateSalesOpportunity(
    Guid OrganizationId,
    Guid CompanyId,
    string Title,
    string Description,
    decimal ExpectedValue,
    CurrencyCode Currency,
    DateTimeOffset? ExpectedCloseDate,
    Guid? OwnerId,
    Guid CreatedBy) : ICreateCommand;
