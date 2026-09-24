using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Forms.Application.Responses.Start;

/// <summary>
/// Not an aggregate handler: it starts a stream, and for a one-per-person form it must first find
/// out whether that stream exists.
/// <para>
/// The form and its version are replayed and loaded by id - never read from the list projection -
/// so a response can be started the second a version is published.
/// </para>
/// </summary>
public static class StartFormResponseHandler
{
    public const string NotOpenMessage = "This form is not open for responses. It has to be published and not archived.";

    public const string WrongSubjectMessage = "This form is not filled in for that kind of record.";

    public static async Task<FormResponseStarted> Handle(
        StartFormResponse command,
        IFormsService service,
        IFormsRepository repository,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);

        var form = await repository.GetFormAsync(command.FormId, ct);
        service.ValidateAggregateUpdate(form, command.OrganizationId, "Form", command.FormId);

        if (!form!.IsOpenForResponses)
            throw new BusinessRuleException(NotOpenMessage);

        if (!SubjectKinds.IsKnown(command.SubjectKind))
            throw new BusinessRuleException(SubjectKinds.UnknownKindMessage);

        if (form.SubjectKind != command.SubjectKind)
            throw new BusinessRuleException(WrongSubjectMessage);

        var subject = new SubjectRef(command.SubjectKind, command.SubjectId);

        // The subject lookup carries the organization; it is what keeps a form away from another
        // agency's worker, since the subject reference itself names no organization.
        var snapshot = await service.GetSubjectAsync(organizationId, subject, ct);

        var responseId = form.Cardinality == ResponseCardinality.OnePerSubject
            ? FormsStreamId.ForSingleResponse(command.OrganizationId, form.Id, subject)
            : Guid.CreateVersion7();

        if (form.Cardinality == ResponseCardinality.OnePerSubject
            && await ExistingStart(session, responseId, ct) is { } existing)
            return existing;

        var version =
            await repository.GetVersionAsync(form.Id, form.PublishedVersion, ct)
            ?? throw new BusinessRuleException(ResponseAnswering.VersionMissingMessage);

        var profile = await repository.GetProfileAsync(command.OrganizationId, subject, ct);
        var catalogue = await repository.GetCatalogueAsync(command.OrganizationId, ct);
        var startedBy = await service.GetUserAsync(command.CreatedBy, ct);

        var @event = new FormResponseStarted(
            command.OrganizationId,
            responseId,
            form.Id,
            form.Code,
            version.FormName,
            version.Version,
            subject.Kind,
            subject.Id,
            ResponsePrefill.For(version.Pages, profile, snapshot.Worker, catalogue),
            startedBy,
            clock.UtcNow
        );

        // Two concurrent starts of a one-per-person form both get here; the second fails on the
        // stream id, which is the point of deriving it.
        session.Events.StartStream<FormResponse>(responseId, @event);

        return @event;
    }

    private static async Task<FormResponseStarted?> ExistingStart(
        IDocumentSession session,
        Guid responseId,
        CancellationToken ct
    )
    {
        var state = await session.Events.FetchStreamStateAsync(responseId, ct);

        if (state is null)
            return null;

        var events = await session.Events.FetchStreamAsync(responseId, 1, token: ct);

        return events.Select(e => e.Data).OfType<FormResponseStarted>().FirstOrDefault();
    }
}
