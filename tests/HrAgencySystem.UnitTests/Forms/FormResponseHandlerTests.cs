using HrAgencySystem.Forms.Application.Responses.Correct;
using HrAgencySystem.Forms.Application.Responses.SaveDraft;
using HrAgencySystem.Forms.Application.Responses.Submit;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using NSubstitute;
using static HrAgencySystem.UnitTests.Forms.FormScenario;

namespace HrAgencySystem.UnitTests.Forms;

public class FormResponseHandlerTests : BaseTest
{
    private static readonly FormField Consent =
        Own("gdpr.consent", FieldType.Boolean, new FieldRules(Required: true));

    private static readonly FormField PhoneOnForm = OnForm(Phone);

    private static readonly FormVersion Version1 = Version([Page(Consent, PhoneOnForm)]);

    [Fact]
    public async Task SaveDraft_LeavesRequiredFieldsForLater()
    {
        var (saved, _) = await SaveFormResponseDraftHandler.Handle(
            new SaveFormResponseDraft(ResponseId, OrganizationId, [Answer("employee.phone", "+48 600 100 200")], User.Id),
            Response(),
            Service(),
            Repository(Version1),
            TestClock,
            CancellationToken.None
        );

        Assert.Single(saved.Answers);
    }

    [Fact]
    public async Task SaveDraft_KeysErrorsByFieldCode()
    {
        var error = await Assert.ThrowsAsync<FieldValidationException>(() =>
            SaveFormResponseDraftHandler.Handle(
                new SaveFormResponseDraft(ResponseId, OrganizationId, [Answer("employee.phone", "call me")], User.Id),
                Response(),
                Service(),
                Repository(Version1),
                TestClock,
                CancellationToken.None
            ));

        Assert.Equal([FormAnswersValidator.PhoneMessage], error.FieldErrors["employee.phone"]);
        Assert.Equal([$"Phone: {FormAnswersValidator.PhoneMessage}"], error.Errors);
    }

    [Fact]
    public async Task SaveDraft_SubmittedResponse_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            SaveFormResponseDraftHandler.Handle(
                new SaveFormResponseDraft(ResponseId, OrganizationId, [], User.Id),
                Response(submitted: true),
                Service(),
                Repository(Version1),
                TestClock,
                CancellationToken.None
            ));

        Assert.Equal(SaveFormResponseDraftHandler.AlreadySubmittedMessage, error.Message);
    }

    [Fact]
    public async Task Submit_AsksForRequiredFields()
    {
        var error = await Assert.ThrowsAsync<FieldValidationException>(() =>
            SubmitFormResponseHandler.Handle(
                new SubmitFormResponse(ResponseId, OrganizationId, [], User.Id),
                Response(),
                Service(),
                Repository(Version1),
                TestClock,
                CancellationToken.None
            ));

        Assert.Equal([FormAnswersValidator.RequiredConsentMessage], error.FieldErrors["gdpr.consent"]);
    }

    [Fact]
    public async Task Submit_PutsOnlySystemFieldsIntoTheProfile()
    {
        var repository = Repository(Version1);

        await SubmitFormResponseHandler.Handle(
            new SubmitFormResponse(ResponseId, OrganizationId, [Consented(), Answer("employee.phone", "600100200")], User.Id),
            Response(),
            Service(),
            repository,
            TestClock,
            CancellationToken.None
        );

        repository.Received(1).StoreProfile(Arg.Is<SubjectProfile>(p =>
            p.Values.Count == 1 && p.Values[0].Code == "employee.phone" && p.Values[0].ResponseId == ResponseId));
    }

    [Fact]
    public async Task Submit_IsCheckedAgainstTheResponsesOwnVersion()
    {
        var repository = Repository(Version1);

        await SubmitFormResponseHandler.Handle(
            new SubmitFormResponse(ResponseId, OrganizationId, [Consented()], User.Id),
            Response(),
            Service(),
            repository,
            TestClock,
            CancellationToken.None
        );

        await repository.Received(1).GetVersionAsync(FormId, 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Correct_WithoutAReason_Refuses()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            CorrectFormResponseHandler.Handle(
                new CorrectFormResponse(ResponseId, OrganizationId, [Consented()], "  ", User.Id),
                Response([Consented()], submitted: true),
                Service(),
                Repository(Version1),
                TestClock,
                CancellationToken.None
            ));

        Assert.Equal(CorrectFormResponseHandler.ReasonRequiredMessage, error.Message);
    }

    [Fact]
    public async Task Correct_ADraft_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CorrectFormResponseHandler.Handle(
                new CorrectFormResponse(ResponseId, OrganizationId, [Consented()], "Typo", User.Id),
                Response(),
                Service(),
                Repository(Version1),
                TestClock,
                CancellationToken.None
            ));

        Assert.Equal(CorrectFormResponseHandler.NotSubmittedMessage, error.Message);
    }

    [Fact]
    public async Task Correct_RaisesTheRevisionAndKeepsTheVersion()
    {
        var (corrected, _) = await CorrectFormResponseHandler.Handle(
            new CorrectFormResponse(ResponseId, OrganizationId, [Consented()], "Signed on paper", User.Id),
            Response([Consented()], submitted: true),
            Service(),
            Repository(Version1),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(1, corrected.Revision);
        Assert.Equal("Signed on paper", corrected.Reason);
    }

    [Fact]
    public async Task Correct_OnlyWhatChangedReachesTheProfile()
    {
        // The phone is re-sent unchanged; bringing it into the profile would overwrite whatever the
        // person declared since in another form.
        var before = new[] { Consented(), Answer("employee.phone", "600100200") };
        var repository = Repository(Version1);

        await CorrectFormResponseHandler.Handle(
            new CorrectFormResponse(ResponseId, OrganizationId, before, "Re-checked", User.Id),
            Response(before, submitted: true),
            Service(),
            repository,
            TestClock,
            CancellationToken.None
        );

        repository.DidNotReceive().StoreProfile(Arg.Any<SubjectProfile>());
    }

    [Fact]
    public void Profile_KeepsTheNewerValue()
    {
        var profile = SubjectProfile.EmptyFor(OrganizationId, new SubjectRef(SubjectKinds.Worker, WorkerId))
            .With([Answer("employee.phone", "new")], Guid.NewGuid(), Yesterday.AddDays(1))
            .With([Answer("employee.phone", "old")], Guid.NewGuid(), Yesterday);

        Assert.Equal("new", profile.ValueOf("employee.phone")!.Value.Text);
    }

    [Fact]
    public void Prefill_ProfileBeatsTheWorkersFile()
    {
        var profile = SubjectProfile.EmptyFor(OrganizationId, new SubjectRef(SubjectKinds.Worker, WorkerId))
            .With([Answer("employee.phone", "111 222 333")], Guid.NewGuid(), Yesterday);

        var prefill = ResponsePrefill.For(Version1.Pages, profile, Worker(), Catalogue);

        Assert.Equal("111 222 333", prefill.Single().Value.Text);
    }

    [Fact]
    public void Prefill_FallsBackToTheWorkersFile()
    {
        var prefill = ResponsePrefill.For(Version1.Pages, null, Worker(), Catalogue);

        Assert.Equal("+48 600 100 200", prefill.Single(a => a.FieldCode == "employee.phone").Value.Text);
    }

    [Fact]
    public void Prefill_UsesAFormFieldsDefault()
    {
        var withDefault = Consent with { DefaultValue = new FieldValue(Boolean: true) };

        var prefill = ResponsePrefill.For([Page(withDefault)], null, null, Catalogue);

        Assert.True(prefill.Single().Value.Boolean);
    }

    private static FieldAnswer Answer(string code, string text) => new(code, FieldValue.OfText(text));

    private static FieldAnswer Consented() => new("gdpr.consent", new FieldValue(Boolean: true));

    private static WorkerSnapshot Worker() =>
        new(WorkerId, "Jan", "Kowalski", new DateOnly(1990, 5, 1), "PL", "jan@example.com", "+48 600 100 200");
}
