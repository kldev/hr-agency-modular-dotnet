using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Application.Opportunities.Create;

public interface IOpportunityData
{
    string Title { get; }
    string Description { get; }
    decimal ExpectedValue { get; }
    CurrencyCode Currency { get; }
    DateTimeOffset? ExpectedCloseDate { get; }
}