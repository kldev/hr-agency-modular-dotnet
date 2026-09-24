using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Forms.Documents;

/// <summary>
/// A published version, frozen: the layout people fill in and the structure every response to it
/// is validated and shown against, for good.
/// <para>
/// Inserted in the same transaction as <c>FormPublished</c>, never by a projection. A response is
/// often started seconds after publication, and a version a daemon had not written yet would refuse
/// it. Its id is derived from <c>(formId, version)</c>, so reading one is a single load and writing
/// the same version twice fails on the key.
/// </para>
/// <para>Never updated, never deleted. System fields inside it are copies, not references.</para>
/// </summary>
public sealed record FormVersion(
    Guid Id,
    Guid OrganizationId,
    Guid FormId,
    string FormCode,
    string FormName,
    FormKind Kind,
    int Version,
    IReadOnlyList<FormPage> Pages,
    UserSnapshot PublishedBy,
    DateTimeOffset PublishedAt
);
