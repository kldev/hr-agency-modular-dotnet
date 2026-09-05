namespace HrAgencySystem.Recruitment.Domain.JobPostings.ValueObjects;
public readonly record struct JobDescriptionId(Guid Value)
{
    public static JobDescriptionId New()
    {
        return new(Guid.NewGuid());
    }

    public static JobDescriptionId From(Guid value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}