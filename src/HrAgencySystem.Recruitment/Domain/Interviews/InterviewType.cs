using JetBrains.Annotations;

namespace HrAgencySystem.Recruitment.Domain.Interviews;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum InterviewType
{
    Hr,
    Technical,
    Client,
    Final,
}
