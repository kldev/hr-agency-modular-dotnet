using JetBrains.Annotations;

namespace HrAgencySystem.SharedKernel.ValueObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)] // Values arrive through the API and stored events.
public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Temporary,
    Internship,
}
