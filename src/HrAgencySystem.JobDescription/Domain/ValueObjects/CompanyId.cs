namespace HrAgencySystem.JobDescription.Domain.ValueObjects;

public readonly record struct CompanyId(Guid Value)
{
    public static CompanyId From(Guid value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
