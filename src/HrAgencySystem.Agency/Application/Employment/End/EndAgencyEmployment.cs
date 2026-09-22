using HrAgencySystem.Agency.Domain;

namespace HrAgencySystem.Agency.Application.Employment.End;

public sealed record EndAgencyEmployment(
    Guid OrganizationId,
    Guid UserId,
    DateOnly EndsOn,
    Guid ModifiedBy
)
{
    public Guid Id => AgencyStreamId.ForEmployment(OrganizationId, UserId);
}
