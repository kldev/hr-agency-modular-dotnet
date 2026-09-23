using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.SystemFields;

namespace HrAgencySystem.Forms.Application.Port;

/// <summary>
/// The write side's reads: the catalogue a layout is resolved against, the frozen version a response
/// is checked against, the profile a submission updates. Replayed or loaded by id - never queried
/// from a read model the daemon may not have caught up with.
/// <para>
/// A port rather than a session in the handlers, so a unit test hands over a catalogue instead of
/// standing up Marten.
/// </para>
/// </summary>
public interface IFormsRepository
{
    /// <summary>The organization's catalogue; an empty list when it has never defined a field.</summary>
    Task<IReadOnlyList<SystemField>> GetCatalogueAsync(Guid organizationId, CancellationToken ct);

    Task<FormDefinition?> GetFormAsync(Guid formId, CancellationToken ct);

    Task<FormVersion?> GetVersionAsync(Guid formId, int version, CancellationToken ct);

    void AddVersion(FormVersion version);

    Task<SubjectProfile?> GetProfileAsync(Guid organizationId, SubjectRef subject, CancellationToken ct);

    void StoreProfile(SubjectProfile profile);
}
