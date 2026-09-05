using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public sealed class CandidateEmailReservationRepository(IDocumentSession session) : ICandidateEmailReservationRepository
{
    public async Task<bool> ExistsAsync(OrganizationId organizationId, Email email, CancellationToken ct)
    {
        return await session.Query<CandidateEmailReservation>().WithEmail(organizationId, email).AnyAsync(ct);
    }

    public Task ReserveAsync(OrganizationId organizationId, Email email, CandidateId id)
    {
        var reservation = new CandidateEmailReservation(Guid.NewGuid(), organizationId.Value, id.Value, email.Value);
        session.Insert(reservation);
        return Task.CompletedTask;
    }
}