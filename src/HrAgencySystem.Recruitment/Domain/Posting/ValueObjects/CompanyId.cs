namespace HrAgencySystem.Recruitment.Domain.Posting.ValueObjects;

public readonly record struct CompanyId(Guid Value)
{
    public static CompanyId New()
    {
        return new(Guid.NewGuid());
    }

    public static CompanyId From(Guid value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}