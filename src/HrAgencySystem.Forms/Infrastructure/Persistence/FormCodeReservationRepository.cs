using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using Marten;

namespace HrAgencySystem.Forms.Infrastructure.Persistence;

// ReSharper disable once ClassNeverInstantiated.Global
// Public: Wolverine generates handler code into another assembly and cannot build an internal type.
public sealed class FormCodeReservationRepository(IDocumentSession session) : IFormCodeReservationRepository
{
    public async Task<bool> ExistsAsync(Guid organizationId, string code, CancellationToken ct) =>
        await session
            .Query<FormCodeReservation>()
            .AnyAsync(r => r.OrganizationId == organizationId && r.Code == code, ct);

    public void Reserve(Guid organizationId, string code, Guid formId) =>
        session.Insert(new FormCodeReservation(Guid.CreateVersion7(), organizationId, code, formId));
}
