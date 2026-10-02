using JetBrains.Annotations;

namespace HrAgencySystem.Projects.Domain;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum ContractStatus
{
    Draft,
    Signed,
    Terminated,
    Expired,
}
