namespace HrAgencySystem.Recruitment.Domain.Candidates;

public readonly record struct CandidateId(Guid Value)
{
    public static CandidateId New()
    {
        return new CandidateId(Guid.NewGuid());
    }

    public static CandidateId From(Guid value)
    {
        return new CandidateId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}