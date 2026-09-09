using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Application.Queries;

public interface ISalesPipelineQueryRepository
{
    public Task<IReadOnlyCollection<SalesPipelineQueryResult>> GetTotalsAsync(Guid organisationId, OpportunityQuery query, CancellationToken ct);
    public Task<IReadOnlyCollection<SalesPipelineResponsibleQueryResult>> GetResponsibleTotalsAsync(Guid organizationId, CancellationToken ct);
}

public record SalesPipelineQueryResult(
    OpportunityStage Stage,
    CurrencyCode CurrencyCode,
    int Count,
    decimal TotalExpectedValue
    );
    
public record SalesPipelineResponsibleQueryResult(
    OpportunityStage Stage,
    CurrencyCode CurrencyCode,
    Guid ResponsibleId,
    int Count,
    decimal TotalExpectedValue,
    UserSnapshot? Responsible);

    