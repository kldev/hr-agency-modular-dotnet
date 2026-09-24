using HrAgencySystem.Forms.Application.FormDefinitions.Archive;
using HrAgencySystem.Forms.Application.FormDefinitions.Create;
using HrAgencySystem.Forms.Application.FormDefinitions.Publish;
using HrAgencySystem.Forms.Application.FormDefinitions.SaveDraft;
using HrAgencySystem.Forms.Application.FormDefinitions.UpdateDetails;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using Marten;
using NSubstitute;
using static HrAgencySystem.UnitTests.Forms.FormScenario;

namespace HrAgencySystem.UnitTests.Forms;

public class FormDefinitionHandlerTests : BaseTest
{
    [Fact]
    public async Task Create_TakenCode_Refuses()
    {
        var reservations = Substitute.For<IFormCodeReservationRepository>();
        reservations.ExistsAsync(OrganizationId, "gdpr-consent", Arg.Any<CancellationToken>()).Returns(true);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateFormDefinitionHandler.Handle(
                Create("GDPR-Consent"),
                Service(),
                reservations,
                Substitute.For<IDocumentSession>(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(CreateFormDefinitionHandler.CodeTakenMessage, error.Message);
    }

    [Fact]
    public async Task Create_ReservesTheCodeAndTakesTheKindsCardinality()
    {
        var reservations = Substitute.For<IFormCodeReservationRepository>();

        var created = await CreateFormDefinitionHandler.Handle(
            Create("satisfaction", FormKind.Survey),
            Service(),
            reservations,
            Substitute.For<IDocumentSession>(),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(ResponseCardinality.Many, created.Cardinality);
        reservations.Received(1).Reserve(OrganizationId, "satisfaction", created.FormId);
    }

    [Fact]
    public async Task Create_InvalidInput_ReportsEveryProblem()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateFormDefinitionHandler.Handle(
                Create("Bad Code") with { Name = " ", SubjectKind = "candidate" },
                Service(),
                Substitute.For<IFormCodeReservationRepository>(),
                Substitute.For<IDocumentSession>(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal([FormCode.FormatMessage, FormName.RequiredMessage, SubjectKinds.UnknownKindMessage], error.Errors);
    }

    [Fact]
    public async Task SaveDraft_WithErrors_KeysThemByField()
    {
        var broken = Own("tax.office", id: Guid.NewGuid()) with { Label = "" };

        var error = await Assert.ThrowsAsync<FieldValidationException>(() =>
            SaveFormDraftHandler.Handle(
                new SaveFormDraft(FormId, OrganizationId, [Page(broken)], User.Id),
                Form(),
                Service(),
                Repository(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal([FormLayoutPolicy.LabelRequiredMessage], error.FieldErrors[broken.FieldId.ToString()]);
    }

    [Fact]
    public async Task SaveDraft_ArchivedForm_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            SaveFormDraftHandler.Handle(
                new SaveFormDraft(FormId, OrganizationId, [], User.Id),
                Form(publishedVersions: 1, archived: true),
                Service(),
                Repository(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(UpdateFormDetailsHandler.ArchivedMessage, error.Message);
    }

    [Fact]
    public async Task Publish_FreezesTheVersionWithTodaysCatalogue()
    {
        // The draft was saved when PESEL was called "PESEL"; the catalogue has renamed it since.
        var draft = Form(draft: [Page(Own("gdpr.consent", FieldType.Boolean), FromCatalogue(PeselFieldId) with { Code = "employee.pesel", Label = "PESEL" })]);
        var repository = Repository();
        repository
            .GetCatalogueAsync(OrganizationId, Arg.Any<CancellationToken>())
            .Returns([Pesel with { Label = "National ID (PESEL)" }, Phone]);

        var (published, _) = await PublishFormHandler.Handle(
            new PublishForm(FormId, OrganizationId, User.Id),
            draft,
            Service(),
            repository,
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(1, published.Version);
        Assert.Equal("National ID (PESEL)", published.Pages.AllFields.Single(f => f.Code == "employee.pesel").Label);
        repository.Received(1).AddVersion(Arg.Is<FormVersion>(v =>
            v.Id == FormsStreamId.ForVersion(FormId, 1) && v.Version == 1 && v.Pages == published.Pages));
    }

    [Fact]
    public async Task Publish_NumbersVersionsOneAfterAnother()
    {
        var form = Form(publishedVersions: 2);
        form.Apply(new HrAgencySystem.Forms.Events.FormDraftSaved(OrganizationId, FormId, form.Pages, User, Yesterday));

        var (published, _) = await PublishFormHandler.Handle(
            new PublishForm(FormId, OrganizationId, User.Id),
            form,
            Service(),
            Repository(),
            TestClock,
            CancellationToken.None
        );

        Assert.Equal(3, published.Version);
    }

    [Fact]
    public async Task Publish_NothingChangedSinceTheLastVersion_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            PublishFormHandler.Handle(
                new PublishForm(FormId, OrganizationId, User.Id),
                Form(publishedVersions: 1),
                Service(),
                Repository(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(PublishFormHandler.NothingToPublishMessage, error.Message);
    }

    [Fact]
    public async Task Publish_IncompleteForm_RefusesAndWritesNoVersion()
    {
        var repository = Repository();

        var error = await Assert.ThrowsAsync<FieldValidationException>(() =>
            PublishFormHandler.Handle(
                new PublishForm(FormId, OrganizationId, User.Id),
                Form(draft: []),
                Service(),
                repository,
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Contains(FormLayoutPolicy.NoPagesMessage, error.Errors);
        repository.DidNotReceive().AddVersion(Arg.Any<FormVersion>());
    }

    [Fact]
    public async Task Archive_Twice_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ArchiveFormHandler.Handle(
                new ArchiveForm(FormId, OrganizationId, User.Id),
                Form(archived: true),
                Service(),
                TestClock,
                CancellationToken.None
            )
        );

        Assert.Equal(ArchiveFormHandler.AlreadyArchivedMessage, error.Message);
    }

    [Fact]
    public void Form_StatusFollowsItsHistory()
    {
        Assert.Equal(FormStatus.Draft, Form().Status);
        Assert.Equal(FormStatus.Published, Form(publishedVersions: 1).Status);
        Assert.Equal(FormStatus.Archived, Form(publishedVersions: 1, archived: true).Status);
        Assert.False(Form(publishedVersions: 1, archived: true).IsOpenForResponses);
    }

    private static CreateFormDefinition Create(string code, FormKind kind = FormKind.Document) =>
        new(OrganizationId, code, "GDPR consent", null, kind, null, SubjectKinds.Worker, User.Id);
}
