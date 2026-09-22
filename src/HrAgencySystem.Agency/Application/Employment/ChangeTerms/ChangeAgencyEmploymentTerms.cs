using HrAgencySystem.Agency.Domain;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Application.Employment.ChangeTerms;

public sealed record ChangeAgencyEmploymentTerms(
    Guid OrganizationId,
    Guid UserId,
    WorkerContractType ContractType,
    DateOnly EffectiveFrom,
    decimal? WeeklyHours,
    Guid ModifiedBy
)
{
    /// <summary>The person's own employment stream, derived from the organization and the user.</summary>
    public Guid Id => AgencyStreamId.ForEmployment(OrganizationId, UserId);
}
