using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.SystemFields;
using Marten;

namespace HrAgencySystem.Forms.Infrastructure.Persistence;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class FormsRepository(IDocumentSession session) : IFormsRepository
{
    public async Task<IReadOnlyList<SystemField>> GetCatalogueAsync(Guid organizationId, CancellationToken ct)
    {
        var catalogue = await session.Events.AggregateStreamAsync<SystemFieldCatalogue>(
            FormsStreamId.ForCatalogue(organizationId),
            token: ct
        );

        return catalogue?.Fields ?? [];
    }

    public async Task<FormDefinition?> GetFormAsync(Guid formId, CancellationToken ct) =>
        await session.Events.AggregateStreamAsync<FormDefinition>(formId, token: ct);

    public async Task<FormVersion?> GetVersionAsync(Guid formId, int version, CancellationToken ct) =>
        await session.LoadAsync<FormVersion>(FormsStreamId.ForVersion(formId, version), ct);

    public void AddVersion(FormVersion version) => session.Insert(version);

    public async Task<SubjectProfile?> GetProfileAsync(
        Guid organizationId,
        SubjectRef subject,
        CancellationToken ct
    ) => await session.LoadAsync<SubjectProfile>(FormsStreamId.ForProfile(organizationId, subject), ct);

    public void StoreProfile(SubjectProfile profile) => session.Store(profile);
}
