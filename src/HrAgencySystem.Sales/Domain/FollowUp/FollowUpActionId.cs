namespace HrAgencySystem.Sales.Domain.FollowUp;

public readonly record struct FollowUpActionId(Guid Value)
{
    public static FollowUpActionId New()
    {
        return new FollowUpActionId(Guid.NewGuid());
    }

    public static FollowUpActionId From(Guid value)
    {
        return new FollowUpActionId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
