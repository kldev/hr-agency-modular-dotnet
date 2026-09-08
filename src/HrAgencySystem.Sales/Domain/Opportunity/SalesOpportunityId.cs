namespace HrAgencySystem.Sales.Domain.Opportunity;

public readonly record struct SalesOpportunityId(Guid Value)
{
    public static SalesOpportunityId New()
    {
        return new SalesOpportunityId(Guid.NewGuid());
    }

    public static SalesOpportunityId From(Guid value)
    {
        return new SalesOpportunityId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}