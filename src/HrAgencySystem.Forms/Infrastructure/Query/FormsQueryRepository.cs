using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Forms.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class FormsQueryRepository(IQuerySession session, IFormsRepository forms) : IFormsQueryRepository
{
    public async Task<SliceResponse<FormDefinitionProjection>> GetForms(
        OrganizationId organizationId,
        FormQuery query,
        CancellationToken ct
    )
    {
        var organization = organizationId.Value;
        var list = session.Query<FormDefinitionProjection>().Where(f => f.OrganizationId == organization);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            list = list.Where(f => f.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                   || f.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (query.Status is { Length: > 0 } status)
            list = list.Where(f => status.Contains(f.Status));

        if (query.Kind is { Length: > 0 } kind)
            list = list.Where(f => kind.Contains(f.Kind));

        return await list.OrderBy(f => f.Name).ThenBy(f => f.Id).ToSlice(query, ct);
    }

    public async Task<FormDefinitionView?> GetForm(OrganizationId organizationId, Guid formId, CancellationToken ct)
    {
        var form = await forms.GetFormAsync(formId, ct);

        if (form is null || form.OrganizationId != organizationId)
            return null;

        var versions = await session
            .Query<FormVersion>()
            .Where(v => v.FormId == formId && v.OrganizationId == organizationId.Value)
            .OrderByDescending(v => v.Version)
            .Select(v => new { v.Version, v.PublishedBy, v.PublishedAt })
            .ToListAsync(ct);

        return new FormDefinitionView(
            form.Id,
            form.Code,
            form.Name,
            form.Description,
            form.Kind,
            form.Cardinality,
            form.SubjectKind,
            form.Status,
            form.PublishedVersion,
            form.HasUnpublishedChanges,
            form.Pages,
            [.. versions.Select(v => new FormVersionSummary(v.Version, v.PublishedBy, v.PublishedAt))],
            form.CreatedBy,
            form.CreatedAt,
            form.ModifiedBy,
            form.ModifiedAt
        );
    }

    public async Task<FormVersion?> GetVersion(
        OrganizationId organizationId,
        Guid formId,
        int version,
        CancellationToken ct
    )
    {
        var document = await forms.GetVersionAsync(formId, version, ct);

        return document?.OrganizationId == organizationId.Value ? document : null;
    }

    public async Task<IReadOnlyList<FormDefinitionProjection>> GetAvailableForms(
        OrganizationId organizationId,
        SubjectRef subject,
        CancellationToken ct
    )
    {
        var organization = organizationId.Value;

        var open = await session
            .Query<FormDefinitionProjection>()
            .Where(f => f.OrganizationId == organization
                        && f.Status == FormStatus.Published
                        && f.SubjectKind == subject.Kind)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

        var answered = await session
            .Query<FormResponseProjection>()
            .Where(r => r.OrganizationId == organization
                        && r.SubjectKind == subject.Kind
                        && r.SubjectId == subject.Id)
            .Select(r => r.FormId)
            .ToListAsync(ct);

        return
        [
            .. open.Where(f => f.Cardinality == ResponseCardinality.Many || !answered.Contains(f.Id)),
        ];
    }

    public async Task<FormLayoutPreview> PreviewLayout(
        OrganizationId organizationId,
        IReadOnlyList<FormPage> pages,
        CancellationToken ct
    )
    {
        var catalogue = await forms.GetCatalogueAsync(organizationId.Value, ct);
        var (prepared, draftErrors) = FormLayoutPolicy.PrepareDraft(pages, catalogue);
        var errors = draftErrors.Concat(FormLayoutPolicy.ValidatePublish(prepared)).ToList();

        return new FormLayoutPreview(
            prepared,
            [.. errors.Select(error => error.ToMessage()).Distinct()],
            errors
                .GroupBy(error => error.Target?.ToString() ?? "")
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<string>)[.. group.Select(error => error.Message).Distinct()]
                ),
            errors.Count == 0
        );
    }

    /// <summary>
    /// Replayed, not projected: the catalogue is tens of events, and the builder must offer the field
    /// that was defined a second ago.
    /// </summary>
    public async Task<IReadOnlyList<SystemField>> GetSystemFields(
        OrganizationId organizationId,
        bool includeArchived,
        CancellationToken ct
    )
    {
        var catalogue = await forms.GetCatalogueAsync(organizationId.Value, ct);

        return [.. catalogue.Where(field => includeArchived || !field.IsArchived).OrderBy(field => field.Code, StringComparer.Ordinal)];
    }
}
