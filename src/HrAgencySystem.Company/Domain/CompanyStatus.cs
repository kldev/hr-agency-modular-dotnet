using JetBrains.Annotations;

namespace HrAgencySystem.Company.Domain;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum CompanyStatus
{
    Active,
    Inactive,
}
