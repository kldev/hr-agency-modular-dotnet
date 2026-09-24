using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Forms;

/// <summary>
/// Forms, catalogues and responses built by replaying the events that would have produced them -
/// never by reaching into an aggregate - so no test sets up a state the commands cannot reach.
/// </summary>
internal static class FormScenario
{
    public static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid FormId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    public static readonly Guid ResponseId = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    public static readonly Guid WorkerId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
    public static readonly Guid PeselFieldId = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001");
    public static readonly Guid PhoneFieldId = Guid.Parse("eeeeeeee-0000-0000-0000-000000000002");
    public static readonly Guid PageId = Guid.Parse("ffffffff-0000-0000-0000-000000000001");

    public static readonly DateTimeOffset Yesterday = new(2026, 9, 22, 10, 0, 0, TimeSpan.Zero);

    public static UserSnapshot User { get; } = new(Guid.NewGuid(), "Anna", "Nowak", "anna.nowak@hr-agency.com");

    public static SystemField Pesel { get; } =
        new(
            PeselFieldId,
            "employee.pesel",
            FieldType.Text,
            "PESEL",
            null,
            new FieldRules(Pattern: StandardSystemFields.PeselPattern),
            [],
            SystemFieldSource.None,
            false
        );

    public static SystemField Phone { get; } =
        new(PhoneFieldId, "employee.phone", FieldType.Phone, "Phone", null, FieldRules.None, [], SystemFieldSource.WorkerPhone, false);

    public static IReadOnlyList<SystemField> Catalogue { get; } = [Pesel, Phone];

    /// <summary>A form field the author writes by hand.</summary>
    public static FormField Own(
        string code,
        FieldType type = FieldType.Text,
        FieldRules? rules = null,
        IReadOnlyList<ChoiceOption>? options = null,
        Guid? id = null
    ) =>
        new(id ?? Guid.NewGuid(), FieldSource.Form, null, code, type, code, null, null, null, rules ?? FieldRules.None, options ?? [], null, null);

    /// <summary>A system field as the builder sends it: an id and nothing else worth trusting.</summary>
    public static FormField FromCatalogue(Guid systemFieldId, string? labelOverride = null, Guid? id = null) =>
        new(id ?? Guid.NewGuid(), FieldSource.System, systemFieldId, "", FieldType.Text, "", labelOverride, null, null, FieldRules.None, [], null, null);

    /// <summary>A system field as a published version holds it: resolved, a copy of the catalogue.</summary>
    public static FormField OnForm(SystemField field) =>
        new(Guid.NewGuid(), FieldSource.System, field.SystemFieldId, field.Code, field.Type, field.Label, null, field.Description, null, field.Rules, field.Options, null, null);

    public static FormPage Page(params FormField[] fields) => new(Guid.NewGuid(), "Page", null, fields);

    public static FormDefinition Form(
        ResponseCardinality cardinality = ResponseCardinality.OnePerSubject,
        IReadOnlyList<FormPage>? draft = null,
        int publishedVersions = 0,
        bool archived = false
    )
    {
        var form = FormDefinition.Empty();

        form.Apply(new FormDefinitionCreated(
            OrganizationId, FormId, "gdpr-consent", "GDPR consent", null, FormKind.Document, cardinality,
            SubjectKinds.Worker, User, Yesterday));

        var pages = draft ?? [Page(Own("gdpr.consent", FieldType.Boolean, new FieldRules(Required: true)))];
        form.Apply(new FormDraftSaved(OrganizationId, FormId, pages, User, Yesterday));

        for (var version = 1; version <= publishedVersions; version++)
            form.Apply(new FormPublished(OrganizationId, FormId, version, pages, User, Yesterday));

        if (archived)
            form.Apply(new FormArchived(OrganizationId, FormId, User, Yesterday));

        return form;
    }

    public static FormVersion Version(IReadOnlyList<FormPage> pages, int version = 1) =>
        new(FormsStreamId.ForVersion(FormId, version), OrganizationId, FormId, "gdpr-consent", "GDPR consent",
            FormKind.Document, version, pages, User, Yesterday);

    public static FormResponse Response(IReadOnlyList<FieldAnswer>? answers = null, bool submitted = false)
    {
        var response = FormResponse.Empty();

        response.Apply(new FormResponseStarted(
            OrganizationId, ResponseId, FormId, "gdpr-consent", "GDPR consent", 1, SubjectKinds.Worker, WorkerId,
            answers ?? [], User, Yesterday));

        if (submitted)
            response.Apply(new FormResponseSubmitted(OrganizationId, ResponseId, answers ?? [], User, Yesterday));

        return response;
    }

    public static IFormsService Service()
    {
        var service = Substitute.For<IFormsService>();

        service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(User);

        return service;
    }

    public static IFormsRepository Repository(FormVersion? version = null, SubjectProfile? profile = null)
    {
        var repository = Substitute.For<IFormsRepository>();

        repository.GetCatalogueAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Catalogue);
        repository.GetVersionAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(version);
        repository.GetProfileAsync(Arg.Any<Guid>(), Arg.Any<SubjectRef>(), Arg.Any<CancellationToken>()).Returns(profile);

        return repository;
    }
}
