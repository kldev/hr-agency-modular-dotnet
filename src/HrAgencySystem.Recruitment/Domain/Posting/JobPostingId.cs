namespace HrAgencySystem.Recruitment.Domain.Posting;

public readonly record struct JobPostingId(Guid Value)
{
    public static JobPostingId New()
    {
        return new(Guid.NewGuid());
    }

    public static JobPostingId From(Guid value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}