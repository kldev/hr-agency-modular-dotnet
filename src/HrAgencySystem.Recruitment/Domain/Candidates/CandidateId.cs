namespace HrAgencySystem.Recruitment.Domain.Candidates;

public readonly record struct CandidateId(Guid Value)
{
    public static CandidateId New()
    {
        return new(Guid.NewGuid());
    }

    public static CandidateId From(Guid value)
    {
        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}