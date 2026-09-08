using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Application.Queries;

public interface ISalesPipelineQueryRepository
{
    public Task<IReadOnlyCollection<SalesPipelineQueryResult>> GetTotalsAsync(Guid organisationId, CancellationToken ct);
}

public record SalesPipelineQueryResult(
    SalesOpportunityStage Stage,
    CurrencyCode CurrencyCode,
    int Count,
    decimal TotalExpectedValue
    );