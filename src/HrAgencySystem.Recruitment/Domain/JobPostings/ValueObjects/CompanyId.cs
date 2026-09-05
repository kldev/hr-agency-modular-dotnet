namespace HrAgencySystem.Recruitment.Domain.JobPostings.ValueObjects;

public readonly record struct CompanyId(Guid Value)
{
    public static CompanyId New()
    {
        return new CompanyId(Guid.NewGuid());
    }

    public static CompanyId From(Guid value)
    {
        return new CompanyId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}