namespace HrAgencySystem.Recruitment.Domain.JobPostings;

public readonly record struct JobPostId(Guid Value)
{
    public static JobPostId New()
    {
        return new JobPostId(Guid.NewGuid());
    }

    public static JobPostId From(Guid value)
    {
        return new JobPostId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}