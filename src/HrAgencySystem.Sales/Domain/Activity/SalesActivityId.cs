namespace HrAgencySystem.Sales.Domain.Activity;


public readonly record struct SalesActivityId(Guid Value)
{
    public static SalesActivityId New()
    {
        return new SalesActivityId(Guid.NewGuid());
    }

    public static SalesActivityId From(Guid value)
    {
        return new SalesActivityId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}