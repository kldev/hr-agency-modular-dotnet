using HrAgencySystem.Projects.Application.Compliance.Record;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.Compliance;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.UnitTests.Projects;

public class RecordComplianceItemHandlerTests : BaseTest
{
    [Fact]
    public async Task Handle_RecordsARequirementFromTheCatalogue()
    {
        var (result, _) = await Handle(
            ProjectScenario.Draft(),
            ComplianceRequirement.BeLimosaDeclaration,
            ComplianceStatus.Confirmed,
            reference: "L1-2026-0001",
            validTo: new DateOnly(2027, 9, 30)
        );

        Assert.Equal(ComplianceRequirement.BeLimosaDeclaration, result.Item.Requirement);
        Assert.Equal("L1-2026-0001", result.Item.ReferenceNumber);
        Assert.True(result.Item.IsSettled);
    }

    [Fact]
    public async Task Handle_NotApplicableCountsAsSettled()
    {
        // Checked and found not to apply is a decision, not a gap.
        var (result, _) = await Handle(
            ProjectScenario.Draft(),
            ComplianceRequirement.BeMotivatedNotification,
            ComplianceStatus.NotApplicable
        );

        Assert.True(result.Item.IsSettled);
    }

    [Fact]
    public async Task Handle_ARequirementOutsideTheCatalogue_Refuses()
    {
        // A Belgian project has no German permit. Ticking one would read as compliance with an
        // obligation nobody has.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                ProjectScenario.Draft(),
                ComplianceRequirement.DeAuegPermit,
                ComplianceStatus.Confirmed,
                reference: "AUG-1"
            )
        );

        Assert.Equal(RecordComplianceItemHandler.NotInCatalogueMessage, error.Message);
    }

    [Fact]
    public async Task Handle_ARequirementOnlyPostingCarries_IsRefusedForAgencyWork()
    {
        var posting = ProjectScenario.Draft(EngagementType.PostingOfWorkers);

        // Posting does not involve a user undertaking, so there is nobody to receive conditions from.
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                posting,
                ComplianceRequirement.UserConditionsReceived,
                ComplianceStatus.Confirmed
            )
        );
    }

    [Fact]
    public async Task Handle_ConfirmingANumberedRequirementWithoutItsNumber_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(
                ProjectScenario.Draft(),
                ComplianceRequirement.BeLimosaDeclaration,
                ComplianceStatus.Confirmed
            )
        );

        Assert.Contains(RecordComplianceItemHandler.ReferenceNumberRequiredMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_ARequirementWithoutANumberNeedsNone()
    {
        var (result, _) = await Handle(
            ProjectScenario.Draft(),
            ComplianceRequirement.A1Certificates,
            ComplianceStatus.Confirmed
        );

        Assert.Null(result.Item.ReferenceNumber);
    }

    [Fact]
    public async Task Handle_MarkingItInProgressNeedsNoNumberEither()
    {
        var (result, _) = await Handle(
            ProjectScenario.Draft(),
            ComplianceRequirement.BeLimosaDeclaration,
            ComplianceStatus.InProgress
        );

        Assert.False(result.Item.IsSettled);
    }

    [Fact]
    public async Task Handle_ValidityEndingBeforeItBegins_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(
                ProjectScenario.Draft(),
                ComplianceRequirement.BeLimosaDeclaration,
                ComplianceStatus.Confirmed,
                reference: "L1-2026-0001",
                validFrom: new DateOnly(2026, 10, 1),
                validTo: new DateOnly(2026, 9, 1)
            )
        );

        Assert.Contains(RecordComplianceItemHandler.ValidToBeforeValidFromMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_PointingAtADocumentThatIsNotOnTheProject_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                ProjectScenario.Draft(),
                ComplianceRequirement.A1Certificates,
                ComplianceStatus.Confirmed,
                documentId: Guid.NewGuid()
            )
        );

        Assert.Equal(RecordComplianceItemHandler.UnknownDocumentMessage, error.Message);
    }

    [Fact]
    public async Task Handle_RecordingTheSameRequirementTwice_Replaces()
    {
        var project = ProjectScenario.Draft();

        var (first, _) = await Handle(
            project,
            ComplianceRequirement.BeLimosaDeclaration,
            ComplianceStatus.InProgress
        );
        project.Apply(first);

        var (second, _) = await Handle(
            project,
            ComplianceRequirement.BeLimosaDeclaration,
            ComplianceStatus.Confirmed,
            reference: "L1-2026-0002"
        );
        project.Apply(second);

        Assert.Single(project.Compliance);
        Assert.Equal("L1-2026-0002", project.Compliance[0].ReferenceNumber);
    }

    private static Task<(ComplianceItemRecorded, Wolverine.Marten.Events)> Handle(
        Project project,
        ComplianceRequirement requirement,
        ComplianceStatus status,
        string? reference = null,
        DateOnly? validFrom = null,
        DateOnly? validTo = null,
        Guid? documentId = null
    ) =>
        RecordComplianceItemHandler.Handle(
            new RecordComplianceItem(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                requirement,
                status,
                reference,
                validFrom,
                validTo,
                documentId,
                null,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
