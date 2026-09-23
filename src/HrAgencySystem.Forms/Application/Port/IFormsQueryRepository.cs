using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Forms.Application.Port;

public sealed record FormQuery(
    string Search,
    FormStatus[]? Status,
    FormKind[]? Kind,
    int Page,
    int PageSize
) : IPagedQuery;

/// <summary>A form as the builder opens it: the working draft and the list of what went out.</summary>
public sealed record FormDefinitionView(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    FormKind Kind,
    ResponseCardinality Cardinality,
    string SubjectKind,
    FormStatus Status,
    int PublishedVersion,
    bool HasUnpublishedChanges,
    IReadOnlyList<FormPage> Pages,
    IReadOnlyList<FormVersionSummary> Versions,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
);

public sealed record FormVersionSummary(int Version, UserSnapshot PublishedBy, DateTimeOffset PublishedAt);

/// <summary>
/// A layout as it would be published, from a draft that may not even be saved: system fields filled
/// from today's catalogue and every problem listed, without refusing anything. It is how the preview
/// renders exactly what publishing would produce.
/// </summary>
public sealed record FormLayoutPreview(
    IReadOnlyList<FormPage> Pages,
    IReadOnlyList<string> Errors,
    IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors,
    bool CanPublish
);

public interface IFormsQueryRepository
{
    Task<SliceResponse<FormDefinitionProjection>> GetForms(
        OrganizationId organizationId,
        FormQuery query,
        CancellationToken ct
    );

    /// <summary>Replayed, not projected: the builder must see the draft it has just saved.</summary>
    Task<FormDefinitionView?> GetForm(OrganizationId organizationId, Guid formId, CancellationToken ct);

    Task<FormVersion?> GetVersion(OrganizationId organizationId, Guid formId, int version, CancellationToken ct);

    /// <summary>
    /// Forms that can be started for this person now: published, not archived, for their kind of
    /// record - minus the one-per-person ones they already have a response to.
    /// </summary>
    Task<IReadOnlyList<FormDefinitionProjection>> GetAvailableForms(
        OrganizationId organizationId,
        SubjectRef subject,
        CancellationToken ct
    );

    Task<FormLayoutPreview> PreviewLayout(
        OrganizationId organizationId,
        IReadOnlyList<FormPage> pages,
        CancellationToken ct
    );

    Task<IReadOnlyList<SystemField>> GetSystemFields(
        OrganizationId organizationId,
        bool includeArchived,
        CancellationToken ct
    );
}
