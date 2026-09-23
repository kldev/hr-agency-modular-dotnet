using System.Text.Json.Nodes;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using Marten;
using Marten.Linq.MatchesSql;

namespace HrAgencySystem.Forms.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class FormResponsesQueryRepository(
    IQuerySession session,
    IDocumentStore store,
    IFormsRepository forms
) : IFormResponsesQueryRepository
{
    public async Task<IReadOnlyList<FormResponseProjection>> GetForSubject(
        OrganizationId organizationId,
        SubjectRef subject,
        CancellationToken ct
    )
    {
        var organization = organizationId.Value;

        return await session
            .Query<FormResponseProjection>()
            .Where(r => r.OrganizationId == organization
                        && r.SubjectKind == subject.Kind
                        && r.SubjectId == subject.Id)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(ct);
    }

    public async Task<FormResponseView?> GetResponse(
        OrganizationId organizationId,
        Guid responseId,
        CancellationToken ct
    )
    {
        var response = await session.Events.AggregateStreamAsync<FormResponse>(responseId, token: ct);

        if (response is null || response.OrganizationId != organizationId)
            return null;

        var version = await forms.GetVersionAsync(response.FormId, response.FormVersion, ct);

        if (version is null)
            return null;

        var events = await session.Events.FetchStreamAsync(responseId, token: ct);

        return new FormResponseView(
            response.Id,
            response.FormId,
            response.FormVersion,
            response.Subject.Kind,
            response.Subject.Id,
            response.Status,
            response.Revision,
            response.Answers,
            version,
            [.. events.Select(e => HistoryOf(e.Data)).OfType<FormResponseHistoryEntry>()],
            response.StartedBy,
            response.StartedAt,
            response.SubmittedBy,
            response.SubmittedAt,
            response.ModifiedBy,
            response.ModifiedAt
        );
    }

    public async Task<SliceResponse<FormResponseProjection>> FindByAnswer(
        OrganizationId organizationId,
        Guid formId,
        FormAnswerQuery query,
        CancellationToken ct
    )
    {
        var organization = organizationId.Value;
        var containment = Containment(query.FieldCode, query.Value);

        var list = session
            .Query<FormResponseProjection>()
            .Where(r => r.OrganizationId == organization && r.FormId == formId)
            .Where(r => r.MatchesSql("d.data @> ?::jsonb", containment));

        if (query.Status is { } status)
            list = list.Where(r => r.Status == status);

        return await list.OrderByDescending(r => r.StartedAt).ThenBy(r => r.Id).ToSlice(query, ct);
    }

    /// <summary>
    /// The JSON the stored document has to contain, written by Marten's own serializer so the
    /// property names and the enum and date formats are whatever it stores - then stripped of the
    /// empty slots, since containment of <c>"Text": null</c> would demand a null that a boolean
    /// answer's document may not spell out.
    /// </summary>
    private string Containment(string fieldCode, FieldValue value)
    {
        var probe = new AnswersProbe(new Dictionary<string, FieldValue> { [fieldCode] = value });
        var node = JsonNode.Parse(store.Options.Serializer().ToJson(probe))!;

        StripNulls(node);

        return node.ToJsonString();
    }

    private static void StripNulls(JsonNode node)
    {
        if (node is not JsonObject obj)
            return;

        foreach (var (key, child) in obj.ToList())
        {
            if (child is null)
                obj.Remove(key);
            else
                StripNulls(child);
        }
    }

    private static FormResponseHistoryEntry? HistoryOf(object data) =>
        data switch
        {
            FormResponseStarted e => new(FormResponseHistoryKind.Started, 0, e.StartedBy, e.StartedAt, null),
            FormResponseDraftSaved e => new(FormResponseHistoryKind.DraftSaved, 0, e.ModifiedBy, e.ModifiedAt, null),
            FormResponseSubmitted e => new(FormResponseHistoryKind.Submitted, 0, e.SubmittedBy, e.SubmittedAt, null),
            FormResponseCorrected e => new(FormResponseHistoryKind.Corrected, e.Revision, e.CorrectedBy, e.CorrectedAt, e.Reason),
            _ => null,
        };

    /// <summary>Only the shape of <see cref="FormResponseProjection.Answers"/>, for the containment probe.</summary>
    private sealed record AnswersProbe(Dictionary<string, FieldValue> Answers);
}
