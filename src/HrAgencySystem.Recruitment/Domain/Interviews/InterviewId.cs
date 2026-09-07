namespace HrAgencySystem.Recruitment.Domain.Interviews;

public readonly record struct InterviewId(Guid Value)
{
    public static InterviewId New()
    {
        return new InterviewId(Guid.NewGuid());
    }

    public static InterviewId From(Guid value)
    {
        return new InterviewId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}