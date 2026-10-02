using JetBrains.Annotations;

namespace HrAgencySystem.Recruitment.Domain.Candidates;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum CandidateStatus
{
    Active,
    Blocked,
    Archived,
}
