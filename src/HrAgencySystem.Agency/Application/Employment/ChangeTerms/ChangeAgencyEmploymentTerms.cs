using HrAgencySystem.Agency.Domain;
using HrAgencySystem.SharedKernel.ValueObjects;
using JetBrains.Annotations;

namespace HrAgencySystem.Agency.Application.Employment.ChangeTerms;

public sealed record ChangeAgencyEmploymentTerms(
    Guid OrganizationId,
    Guid UserId,
    WorkerContractType ContractType,
    DateOnly EffectiveFrom,
    decimal? WeeklyHours,
    Start.RateInput? Rate,
    bool MayQuoteRate,
    Guid ModifiedBy
)
{
    /// <summary>The person's own employment stream, derived from the organization and the user.</summary>
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => AgencyStreamId.ForEmployment(OrganizationId, UserId);
}
