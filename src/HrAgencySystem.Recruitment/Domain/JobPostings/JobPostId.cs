namespace HrAgencySystem.Recruitment.Domain.JobPostings;

public readonly record struct JobPostId(Guid Value)
{
    public static JobPostId New()
    {
        return new(Guid.NewGuid());
    }

    public static JobPostId From(Guid value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}