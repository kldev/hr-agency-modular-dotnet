using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface ICandidateEmailReservationRepository
{
    public const string EmailAlreadyExistsMessage =
        "The candidate with the specified email already exists in this organization.";
    
    Task<bool> ExistsAsync(OrganizationId organizationId, Email email, CancellationToken ct);
    Task ReserveAsync(OrganizationId organizationId, Email email, CandidateId id, CancellationToken ct);
}