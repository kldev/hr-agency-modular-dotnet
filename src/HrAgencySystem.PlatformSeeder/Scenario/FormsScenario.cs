using HrAgencySystem.Forms.Application.FormDefinitions.Create;
using HrAgencySystem.Forms.Application.FormDefinitions.Publish;
using HrAgencySystem.Forms.Application.FormDefinitions.SaveDraft;
using HrAgencySystem.Forms.Application.Responses.SaveDraft;
using HrAgencySystem.Forms.Application.Responses.Start;
using HrAgencySystem.Forms.Application.Responses.Submit;
using HrAgencySystem.Forms.Application.SystemFields.AddStandard;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Events;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// Three forms that between them show every shape the module has: a one-page consent (a plain form,
/// one per person), a four-page personal questionnaire built mostly from system fields (a wizard),
/// and a survey with no system field at all (many per person). A handful of responses - submitted,
/// half filled - so the worker's "Forms" tab and the search by answer have something to show.
/// <para>
/// Skipped when the organization already has forms: codes are reserved, and a second run would
/// only trip over its own first one.
/// </para>
/// </summary>
internal sealed class FormsScenario(IMessageBus bus, IDocumentSession session)
{
    private static readonly ChoiceOption[] Scale =
    [
        new("1", "1 - very poor"),
        new("2", "2"),
        new("3", "3"),
        new("4", "4"),
        new("5", "5 - excellent"),
    ];

    internal async Task<int> Seed(Guid organizationId, Guid createdBy, IReadOnlyList<Guid> workerIds, DateOnly today)
    {
        if (await session.Query<FormCodeReservation>().AnyAsync(r => r.OrganizationId == organizationId))
            return 0;

        await bus.InvokeAsync<StandardSystemFieldsAdded>(new AddStandardSystemFields(organizationId, createdBy));

        var catalogue = (
            await session.Events.AggregateStreamAsync<SystemFieldCatalogue>(FormsStreamId.ForCatalogue(organizationId))
        )!.Fields.ToDictionary(field => field.Code, field => field.SystemFieldId);

        FormField System(string code, string? label = null) =>
            new(Guid.NewGuid(), FieldSource.System, catalogue[code], code, FieldType.Text, "", label, null, null, FieldRules.None, [], null, null);

        var consent = await Publish(
            organizationId,
            createdBy,
            new CreateFormDefinition(organizationId, "gdpr-consent", "GDPR consent", "Consent to the processing of personal data for recruitment and employment.", FormKind.Document, null, SubjectKinds.Worker, createdBy),
            Page("Consent",
                Own("gdpr.consent", FieldType.Boolean, "I consent to the processing of my personal data for the purposes of recruitment and employment.", new FieldRules(Required: true)),
                Own("gdpr.marketingConsent", FieldType.Boolean, "I agree to receive job offers by e-mail."),
                Own("gdpr.consentDate", FieldType.Date, "Date of consent", new FieldRules(Required: true)))
        );

        var questionnaire = await Publish(
            organizationId,
            createdBy,
            new CreateFormDefinition(organizationId, "personal-questionnaire", "Personal questionnaire", "What payroll needs before the first day of work.", FormKind.Document, null, SubjectKinds.Worker, createdBy),
            Page("Personal data",
                System("employee.firstName"),
                System("employee.lastName"),
                System("employee.dateOfBirth"),
                System("employee.pesel"),
                System("employee.citizenship")),
            Page("Contact", System("employee.email"), System("employee.phone")),
            Page("Tax",
                Own("tax.office", FieldType.Text, "Tax office", new FieldRules(Required: true, MaxLength: 200)),
                Own("tax.residence", FieldType.Country, "Country of tax residence", new FieldRules(Required: true)),
                Own("tax.formType", FieldType.SingleChoice, "Tax declaration", new FieldRules(Required: true),
                    [new("pit2", "PIT-2 - I apply the tax-free amount here"), new("none", "No declaration")])),
            Page("Payment",
                System("employee.bankAccount"),
                Own("statement.truthful", FieldType.Boolean, "I declare that the data above is true.", new FieldRules(Required: true)))
        );

        var survey = await Publish(
            organizationId,
            createdBy,
            new CreateFormDefinition(organizationId, "satisfaction-survey", "Satisfaction survey", "Asked after every project.", FormKind.Survey, null, SubjectKinds.Worker, createdBy),
            Page("Your opinion",
                Own("survey.recruitment", FieldType.SingleChoice, "How do you rate the recruitment process?", new FieldRules(Required: true), Scale),
                Own("survey.caretaker", FieldType.SingleChoice, "How do you rate the contact with your caretaker?", new FieldRules(Required: true), Scale),
                Own("survey.recommend", FieldType.Boolean, "Would you recommend us to a friend?"),
                Own("survey.comment", FieldType.TextArea, "Comment", new FieldRules(MaxLength: 2000)))
        );

        var responses = 0;

        foreach (var (workerId, index) in workerIds.Take(4).Select((id, i) => (id, i)))
        {
            await Submit(organizationId, createdBy, consent, workerId,
                new FieldAnswer("gdpr.consent", new FieldValue(Boolean: true)),
                new FieldAnswer("gdpr.marketingConsent", new FieldValue(Boolean: index % 2 == 0)),
                new FieldAnswer("gdpr.consentDate", FieldValue.OfDate(today.AddDays(-10 * (index + 1)))));
            responses++;
        }

        if (workerIds.Count > 0)
        {
            var started = await Start(organizationId, createdBy, questionnaire, workerIds[0]);
            await bus.InvokeAsync<FormResponseSubmitted>(new SubmitFormResponse(started.ResponseId, organizationId,
            [
                .. started.Prefill,
                new FieldAnswer("employee.pesel", FieldValue.OfText("90051201238")),
                new FieldAnswer("tax.office", FieldValue.OfText("Drugi Urząd Skarbowy Warszawa-Śródmieście")),
                new FieldAnswer("tax.residence", FieldValue.OfText("PL")),
                new FieldAnswer("tax.formType", FieldValue.OfText("pit2")),
                new FieldAnswer("employee.bankAccount", FieldValue.OfText("PL61109010140000071219812874")),
                new FieldAnswer("statement.truthful", new FieldValue(Boolean: true)),
            ], createdBy));
            responses++;
        }

        if (workerIds.Count > 1)
        {
            // Left on the tax page: what a questionnaire looks like when somebody stopped half way.
            var started = await Start(organizationId, createdBy, questionnaire, workerIds[1]);
            await bus.InvokeAsync<FormResponseDraftSaved>(new SaveFormResponseDraft(started.ResponseId, organizationId,
                [.. started.Prefill, new FieldAnswer("tax.office", FieldValue.OfText("Urząd Skarbowy Kraków-Podgórze"))], createdBy));
            responses++;
        }

        foreach (var (workerId, index) in workerIds.Take(3).Select((id, i) => (id, i)))
        {
            await Submit(organizationId, createdBy, survey, workerId,
                new FieldAnswer("survey.recruitment", FieldValue.OfText($"{5 - index}")),
                new FieldAnswer("survey.caretaker", FieldValue.OfText($"{4 - index % 2}")),
                new FieldAnswer("survey.recommend", new FieldValue(Boolean: index < 2)),
                new FieldAnswer("survey.comment", FieldValue.OfText(index == 2 ? "Accommodation was far from the site." : "")));
            responses++;
        }

        return responses;
    }

    private async Task<Guid> Publish(Guid organizationId, Guid createdBy, CreateFormDefinition create, params FormPage[] pages)
    {
        var created = await bus.InvokeAsync<FormDefinitionCreated>(create);

        await bus.InvokeAsync<FormDraftSaved>(new SaveFormDraft(created.FormId, organizationId, pages, createdBy));
        await bus.InvokeAsync<FormPublished>(new PublishForm(created.FormId, organizationId, createdBy));

        return created.FormId;
    }

    private Task<FormResponseStarted> Start(Guid organizationId, Guid createdBy, Guid formId, Guid workerId) =>
        bus.InvokeAsync<FormResponseStarted>(new StartFormResponse(organizationId, formId, SubjectKinds.Worker, workerId, createdBy));

    private async Task Submit(Guid organizationId, Guid createdBy, Guid formId, Guid workerId, params FieldAnswer[] answers)
    {
        var started = await Start(organizationId, createdBy, formId, workerId);

        await bus.InvokeAsync<FormResponseSubmitted>(new SubmitFormResponse(started.ResponseId, organizationId, answers, createdBy));
    }

    private static FormPage Page(string title, params FormField[] fields) => new(Guid.NewGuid(), title, null, fields);

    private static FormField Own(
        string code,
        FieldType type,
        string label,
        FieldRules? rules = null,
        IReadOnlyList<ChoiceOption>? options = null
    ) => new(Guid.NewGuid(), FieldSource.Form, null, code, type, label, null, null, null, rules ?? FieldRules.None, options ?? [], null, null);
}
