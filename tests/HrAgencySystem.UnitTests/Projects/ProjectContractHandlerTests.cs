using HrAgencySystem.Projects.Application.Contract.ChangeStatus;
using HrAgencySystem.Projects.Application.Contract.Record;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.UnitTests.Projects;

public class ProjectContractHandlerTests : BaseTest
{
    private static readonly DateOnly SignedOn = new(2026, 9, 15);
    private static readonly DateOnly ValidFrom = new(2026, 10, 1);

    [Fact]
    public async Task Record_KeepsTheNumberAndTheDates()
    {
        var (result, _) = await Record(ProjectScenario.Draft());

        Assert.Equal("UM/2026/17", result.Contract.ContractNumber);
        Assert.Equal(SignedOn, result.Contract.SignedOn);
        Assert.Equal(ValidFrom, result.Contract.ValidFrom);
        Assert.True(result.Contract.IsSigned);
    }

    [Fact]
    public async Task Record_FreezesTheClientAsTheContractNamesThem()
    {
        // The one piece of client data that must not follow the company record: if ACME is later
        // renamed or moves, this contract still says what it said.
        var (result, _) = await Record(ProjectScenario.Draft());

        Assert.Equal("ACME Corporation sp. z o.o.", result.Contract.Party.LegalName);
        Assert.Equal("PL1234567890", result.Contract.Party.TaxId);
        Assert.Equal("Warszawa", result.Contract.Party.RegisteredAddress.City);
    }

    [Fact]
    public async Task Record_TakesTheSignatoryFromTheContactInThatRole()
    {
        var project = ProjectScenario.Draft();
        project.Apply(
            new ProjectContactAssigned(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                ContactRole.ContractSignatory,
                ProjectScenario.Signatory,
                null,
                null,
                ProjectScenario.User,
                DateTimeOffset.UtcNow
            )
        );

        var (result, _) = await Record(project);

        Assert.Equal("Paul", result.Contract.SignedBy!.FirstName);
    }

    [Fact]
    public async Task Record_WithoutASignatoryContact_IsStillAllowed()
    {
        // Who signed is worth recording when we know it, and not knowing yet is not a reason to
        // refuse the contract number.
        var (result, _) = await Record(ProjectScenario.Draft());

        Assert.Null(result.Contract.SignedBy);
    }

    [Fact]
    public async Task Record_AgainstAnIncompleteClientProfile_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Record(
                ProjectScenario.Draft(),
                service: ProjectScenario.Service(ProjectScenario.IncompleteCompany)
            )
        );

        Assert.Equal(RecordProjectContractHandler.PartyProfileRequiredMessage, error.Message);
    }

    [Fact]
    public async Task Record_WithoutANumber_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Record(ProjectScenario.Draft(), number: "  ")
        );

        Assert.Contains(RecordProjectContractHandler.ContractNumberRequiredMessage, error.Errors);
    }

    [Fact]
    public async Task Record_ExpiringBeforeItStarts_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Record(ProjectScenario.Draft(), validTo: ValidFrom.AddDays(-1))
        );

        Assert.Contains(RecordProjectContractHandler.ValidToBeforeValidFromMessage, error.Errors);
    }

    [Fact]
    public async Task Record_SignedAfterItTookEffect_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Record(ProjectScenario.Draft(), signedOn: ValidFrom.AddDays(1))
        );

        Assert.Contains(RecordProjectContractHandler.SignedAfterValidFromMessage, error.Errors);
    }

    [Fact]
    public async Task Record_SignedWithoutASignatureDate_ThrowsValidation()
    {
        // Built inline rather than through the helper: the helper fills in a signature date for a
        // signed contract, which is exactly the convenience this test has to bypass.
        var command = new RecordProjectContract(
            ProjectScenario.ProjectId,
            ProjectScenario.OrganizationId,
            "UM/2026/17",
            ContractStatus.Signed,
            null,
            ValidFrom,
            null,
            ProjectScenario.UserId
        );

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            RecordProjectContractHandler.Handle(
                command,
                ProjectScenario.Draft(),
                ProjectScenario.Service(),
                new FixedClock(DateTimeOffset.UtcNow),
                CancellationToken.None
            )
        );

        Assert.Contains(RecordProjectContractHandler.SignedOnRequiredMessage, error.Errors);
    }

    [Fact]
    public async Task Record_AsADraft_NeedsNoSignatureDate()
    {
        var (result, _) = await Record(
            ProjectScenario.Draft(),
            status: ContractStatus.Draft,
            signedOn: null
        );

        Assert.Equal(ContractStatus.Draft, result.Contract.Status);
        Assert.Null(result.Contract.SignedOn);
    }

    [Fact]
    public async Task ChangeStatus_WithoutAContract_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ChangeStatus(ProjectScenario.Draft(), ContractStatus.Signed, SignedOn)
        );

        Assert.Equal(ChangeContractStatusHandler.NoContractMessage, error.Message);
    }

    [Fact]
    public async Task ChangeStatus_ToTheSameStatus_Refuses()
    {
        var project = ProjectScenario.Draft().WithSignedContract();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ChangeStatus(project, ContractStatus.Signed, SignedOn)
        );

        Assert.Equal(ChangeContractStatusHandler.SameStatusMessage, error.Message);
    }

    [Fact]
    public async Task ChangeStatus_ToTerminated_KeepsTheOriginalSignatureDate()
    {
        var project = ProjectScenario.Draft().WithSignedContract();

        var (result, _) = await ChangeStatus(project, ContractStatus.Terminated, null);

        Assert.Equal(ContractStatus.Signed, result.PreviousStatus);
        Assert.Equal(ContractStatus.Terminated, result.Status);
        Assert.Equal(SignedOn, result.SignedOn);
    }

    private static Task<(ProjectContractRecorded, Wolverine.Marten.Events)> Record(
        Project project,
        string number = "UM/2026/17",
        ContractStatus status = ContractStatus.Signed,
        DateOnly? signedOn = null,
        DateOnly? validTo = null,
        HrAgencySystem.Projects.Services.IProjectService? service = null
    ) =>
        RecordProjectContractHandler.Handle(
            new RecordProjectContract(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                number,
                status,
                status is ContractStatus.Signed && signedOn is null ? SignedOn : signedOn,
                ValidFrom,
                validTo,
                ProjectScenario.UserId
            ),
            project,
            service ?? ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );

    private static Task<(ProjectContractStatusChanged, Wolverine.Marten.Events)> ChangeStatus(
        Project project,
        ContractStatus status,
        DateOnly? signedOn
    ) =>
        ChangeContractStatusHandler.Handle(
            new ChangeContractStatus(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                status,
                signedOn,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
