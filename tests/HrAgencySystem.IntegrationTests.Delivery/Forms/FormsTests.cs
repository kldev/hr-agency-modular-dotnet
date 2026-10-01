using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.Forms.Projections;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.SharedKernel.Web;
using Xunit.Abstractions;
using static HrAgencySystem.IntegrationTests.Forms.FormsTestData;

namespace HrAgencySystem.IntegrationTests.Forms;

[Collection(IntegrationCollection.Name)]
public class FormsTests(IntegrationEnvironment env, ITestOutputHelper output) : BaseIntegrationTest(env, output)
{
    private FormsTestClient Forms { get; } = new(env.CreateClient().AsOrganizationRoles(), output);

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanForms();
        await Cleaner.CleanWorkers();
    }

    [Fact]
    public async Task A_multi_page_form_is_built_published_filled_submitted_and_corrected()
    {
        var organizationId = Guid.NewGuid();
        var catalogue = await Forms.AddStandardFieldsAsync(organizationId);
        var firstName = catalogue.Single(f => f.Code == "employee.firstName");
        var pesel = catalogue.Single(f => f.Code == "employee.pesel");

        var formId = await Forms.PublishedAsync(
            organizationId,
            CreateRequest(),
            Page("Personal data", FromCatalogue(firstName.SystemFieldId), FromCatalogue(pesel.SystemFieldId)),
            Page("Tax", Own("tax.office", rules: new FieldRules(Required: true))),
            Page("Consents", Consent())
        );
        var worker = await WorkerClient.RegisterAsync(organizationId);

        var started = await Forms.StartAsync(organizationId, formId, worker.WorkerId);

        // The first name comes from the worker's file; nothing else is known about them yet.
        Assert.Equal("Jan", Assert.Single(started.Prefill).Value.Text);

        var answers = new[]
        {
            Text("employee.firstName", "Jan"),
            Text("employee.pesel", "90051201234"),
            Text("tax.office", "US Warszawa-Mokotów"),
            Consented(),
        };

        (await Forms.SaveAnswersRawAsync(organizationId, started.ResponseId, answers[..2])).EnsureSuccessStatusCode();
        await Forms.SubmitAsync(organizationId, started.ResponseId, answers);

        var correction = await Forms.CorrectRawAsync(
            organizationId,
            started.ResponseId,
            "The tax office moved",
            answers[0],
            answers[1],
            Text("tax.office", "US Warszawa-Ursynów"),
            Consented()
        );
        correction.EnsureSuccessStatusCode();

        var response = await Forms.ResponseAsync(organizationId, started.ResponseId);

        Assert.NotNull(response);
        Assert.Equal(FormResponseStatus.Submitted, response.Status);
        Assert.Equal(1, response.Revision);
        Assert.Equal(3, response.Version.Pages.Count);
        Assert.Equal("US Warszawa-Ursynów", response.Answers.Single(a => a.FieldCode == "tax.office").Value.Text);
        Assert.Equal("The tax office moved", response.History.Single(h => h.Kind == FormResponseHistoryKind.Corrected).Reason);
    }

    [Fact]
    public async Task A_response_stays_on_the_version_it_was_given_to()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(cardinality: ResponseCardinality.Many),
            Page("Consents", Consent(), Own("gdpr.marketing", FieldType.Boolean)));

        var onVersion1 = await Forms.StartAsync(organizationId, formId, worker.WorkerId);
        await Forms.SubmitAsync(organizationId, onVersion1.ResponseId, Consented(), new FieldAnswer("gdpr.marketing", new FieldValue(Boolean: true)));

        // Version 2 drops the marketing consent.
        await Forms.SaveDraftAsync(organizationId, formId, Page("Consents", Consent()));
        var published = await Forms.PublishAsync(organizationId, formId);
        Assert.Equal(2, published.Version);

        var old = await Forms.ResponseAsync(organizationId, onVersion1.ResponseId);
        Assert.NotNull(old);
        Assert.Equal(1, old.FormVersion);
        Assert.Contains(old.Version.Pages.AllFields, f => f.Code == "gdpr.marketing");
        Assert.Contains(old.Answers, a => a.FieldCode == "gdpr.marketing");

        var onVersion2 = await Forms.StartAsync(organizationId, formId, worker.WorkerId);
        Assert.Equal(2, onVersion2.FormVersion);
    }

    [Fact]
    public async Task A_catalogue_change_reaches_the_draft_but_not_a_published_version()
    {
        var organizationId = Guid.NewGuid();
        var pesel = (await Forms.AddStandardFieldsAsync(organizationId)).Single(f => f.Code == "employee.pesel");
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Data", FromCatalogue(pesel.SystemFieldId)));

        Forms.Http.WithOrganizationId(organizationId);
        (await Forms.Http.PutAsJsonAsync(
            $"/api/system-fields/{pesel.SystemFieldId}",
            new Api.Endpoints.SystemFields.Maps.MapUpdate.UpdateSystemFieldRequest(
                "National ID (PESEL)", null, pesel.Rules, [], pesel.Source)
        )).EnsureSuccessStatusCode();

        var version1 = await Forms.VersionAsync(organizationId, formId, 1);
        Assert.Equal("PESEL", Assert.Single(version1!.Pages.AllFields).Label);

        await Forms.SaveDraftAsync(organizationId, formId, Page("Data", FromCatalogue(pesel.SystemFieldId)));
        var draft = await Forms.GetAsync(organizationId, formId);
        Assert.Equal("National ID (PESEL)", Assert.Single(draft!.Pages.AllFields).Label);
        Assert.True(draft.HasUnpublishedChanges);
    }

    [Fact]
    public async Task A_value_given_in_one_form_prefills_the_next()
    {
        var organizationId = Guid.NewGuid();
        var pesel = (await Forms.AddStandardFieldsAsync(organizationId)).Single(f => f.Code == "employee.pesel");
        var worker = await WorkerClient.RegisterAsync(organizationId);

        var first = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Data", FromCatalogue(pesel.SystemFieldId)));
        var second = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Tax", FromCatalogue(pesel.SystemFieldId)));

        var response = await Forms.StartAsync(organizationId, first, worker.WorkerId);
        await Forms.SubmitAsync(organizationId, response.ResponseId, Text("employee.pesel", "90051201234"));

        var next = await Forms.StartAsync(organizationId, second, worker.WorkerId);

        Assert.Equal("90051201234", Assert.Single(next.Prefill).Value.Text);
    }

    [Fact]
    public async Task A_second_start_of_a_one_per_person_form_opens_the_first_response()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Consents", Consent()));

        var first = await Forms.StartAsync(organizationId, formId, worker.WorkerId);
        var second = await Forms.StartAsync(organizationId, formId, worker.WorkerId);

        Assert.Equal(first.ResponseId, second.ResponseId);
    }

    [Fact]
    public async Task Submitting_without_a_required_field_names_the_field()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Consents", Consent()));
        var started = await Forms.StartAsync(organizationId, formId, worker.WorkerId);

        var response = await Forms.SubmitRawAsync(organizationId, started.ResponseId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<BadRequestDetails>(OutputHelper);
        Assert.Equal([FormAnswersValidator.RequiredConsentMessage], problem!.FieldErrors!["gdpr.consent"]);
        Assert.Equal([$"gdpr.consent: {FormAnswersValidator.RequiredConsentMessage}"], problem.ValidationErrors);
    }

    [Fact]
    public async Task A_plain_validation_error_carries_no_field_errors()
    {
        var organizationId = Guid.NewGuid();

        Forms.Http.WithOrganizationId(organizationId);
        var response = await Forms.Http.PostAsJsonAsync("/api/forms", CreateRequest(code: "Not A Code"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.TryGetProperty("validationErrors", out _));
        Assert.False(body.RootElement.TryGetProperty("fieldErrors", out _));
    }

    [Fact]
    public async Task A_broken_draft_is_refused_with_the_field_marked()
    {
        var organizationId = Guid.NewGuid();
        var formId = await Forms.CreateAsync(organizationId);
        var first = Own("tax.office");
        var second = Own("tax.office");

        var response = await Forms.SaveDraftRawAsync(organizationId, formId, Page("Tax", first, second));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<BadRequestDetails>(OutputHelper);
        Assert.Equal([FormLayoutPolicy.DuplicateCodeMessage], problem!.FieldErrors![first.FieldId.ToString()]);
        Assert.Equal([FormLayoutPolicy.DuplicateCodeMessage], problem.FieldErrors[second.FieldId.ToString()]);
    }

    [Fact]
    public async Task Responses_can_be_found_by_an_answer()
    {
        var organizationId = Guid.NewGuid();
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Consents", Consent(), Own("gdpr.marketing", FieldType.Boolean)));
        var yes = await WorkerClient.RegisterAsync(organizationId, firstName: "Ewa", lastName: "Tak");
        var no = await WorkerClient.RegisterAsync(organizationId, firstName: "Piotr", lastName: "Nie");

        var agreed = await Forms.StartAsync(organizationId, formId, yes.WorkerId);
        await Forms.SubmitAsync(organizationId, agreed.ResponseId, Consented(), new FieldAnswer("gdpr.marketing", new FieldValue(Boolean: true)));
        var refused = await Forms.StartAsync(organizationId, formId, no.WorkerId);
        await Forms.SubmitAsync(organizationId, refused.ResponseId, Consented());

        await Eventually.AssertAsync(async () =>
        {
            Forms.Http.WithOrganizationId(organizationId);
            var found = await (await Forms.Http.GetAsync($"/api/forms/{formId}/responses?field=gdpr.marketing&boolean=true"))
                .ReadWithJson<SliceResponse<FormResponseProjection>>(OutputHelper);

            Assert.Equal(yes.WorkerId, Assert.Single(found!.Content).SubjectId);
        });
    }

    [Fact]
    public async Task A_workers_tab_lists_their_responses()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Consents", Consent()));
        var started = await Forms.StartAsync(organizationId, formId, worker.WorkerId);

        await Eventually.AssertAsync(async () =>
        {
            var row = Assert.Single(await Forms.ForWorkerAsync(organizationId, worker.WorkerId));
            Assert.Equal(started.ResponseId, row.Id);
            Assert.Equal(FormResponseStatus.Draft, row.Status);
        });
    }

    [Fact]
    public async Task Another_organization_sees_nothing_and_changes_nothing()
    {
        var organizationId = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Consents", Consent()));
        var started = await Forms.StartAsync(organizationId, formId, worker.WorkerId);

        Assert.Null(await Forms.GetAsync(stranger, formId));
        Assert.Null(await Forms.ResponseAsync(stranger, started.ResponseId));
        Assert.Equal(HttpStatusCode.NotFound, (await Forms.StartRawAsync(stranger, formId, worker.WorkerId)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Forms.SubmitRawAsync(stranger, started.ResponseId, Consented())).StatusCode);
        Assert.Empty(await Forms.SystemFieldsAsync(stranger));

        // A stranger's own published form cannot be started for this organization's worker either.
        var theirs = await Forms.PublishedAsync(stranger, CreateRequest(), Page("Consents", Consent()));
        Assert.Equal(HttpStatusCode.NotFound, (await Forms.StartRawAsync(stranger, theirs, worker.WorkerId)).StatusCode);
    }

    [Fact]
    public async Task A_recruiter_fills_forms_in_but_does_not_design_or_correct_them()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Consents", Consent()));

        var recruiter = new FormsTestClient(Env.CreateClient(), OutputHelper);
        recruiter.Http.SetTestRoles(nameof(OrganizationRole.Recruiter));
        recruiter.Http.WithOrganizationId(organizationId);

        var started = await recruiter.StartAsync(organizationId, formId, worker.WorkerId);
        await recruiter.SubmitAsync(organizationId, started.ResponseId, Consented());

        Assert.Equal(HttpStatusCode.Forbidden, (await recruiter.Http.PostAsJsonAsync("/api/forms", CreateRequest())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await recruiter.Http.PostAsync($"/api/forms/{formId}/publish", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await recruiter.Http.PostAsync("/api/system-fields/standard", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await recruiter.CorrectRawAsync(organizationId, started.ResponseId, "Typo", Consented())).StatusCode);
    }

    [Fact]
    public async Task An_archived_form_takes_no_new_responses_but_keeps_its_drafts()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);
        var other = await WorkerClient.RegisterAsync(organizationId, firstName: "Anna", lastName: "Wiśniewska");
        var formId = await Forms.PublishedAsync(organizationId, CreateRequest(), Page("Consents", Consent()));
        var started = await Forms.StartAsync(organizationId, formId, worker.WorkerId);

        Forms.Http.WithOrganizationId(organizationId);
        (await Forms.Http.PostAsync($"/api/forms/{formId}/archive", null)).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.BadRequest, (await Forms.StartRawAsync(organizationId, formId, other.WorkerId)).StatusCode);
        await Forms.SubmitAsync(organizationId, started.ResponseId, Consented());
    }
}
