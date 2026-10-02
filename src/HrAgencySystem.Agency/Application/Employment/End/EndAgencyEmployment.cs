using HrAgencySystem.Agency.Domain;
using JetBrains.Annotations;

namespace HrAgencySystem.Agency.Application.Employment.End;

public sealed record EndAgencyEmployment(
    Guid OrganizationId,
    Guid UserId,
    DateOnly EndsOn,
    Guid ModifiedBy
)
{
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => AgencyStreamId.ForEmployment(OrganizationId, UserId);
}
