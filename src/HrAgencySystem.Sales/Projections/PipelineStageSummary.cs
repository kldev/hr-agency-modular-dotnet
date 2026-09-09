using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Projections;

public sealed class PipelineStageSummary
{
    public String Id { get; set; } = "";
    public Guid OrgId { get; set; }

    public OpportunityStage Stage { get; set; }

    public CurrencyCode CurrencyCode { get; set; }

    public int OpportunityCount { get; set; }

    public decimal TotalExpectedValue { get; set; }
}