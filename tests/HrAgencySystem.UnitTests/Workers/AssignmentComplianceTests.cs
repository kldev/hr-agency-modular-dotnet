using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.AssignmentCompliance.Record;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.UnitTests.Workers;

/// <summary>
/// The register CLAUDE.md said did not exist: an A1 recorded against one named person for one named
/// period, instead of one tick on a project claiming everybody is covered.
/// </summary>
public class AssignmentComplianceTests : BaseTest
{
    [Fact]
    public async Task Handle_RecordsAnA1AgainstThePersonAndThePeriod()
    {
        var (result, _) = await Handle(
            WorkerScenario.Planned(),
            ComplianceRequirement.A1Certificates,
            ComplianceStatus.Confirmed,
            validFrom: WorkerScenario.StartsOn,
            validTo: WorkerScenario.StartsOn.AddMonths(12)
        );

        Assert.Equal(WorkerScenario.WorkerId, result.WorkerId);
        Assert.Equal(ComplianceRequirement.A1Certificates, result.Item.Requirement);
        Assert.True(result.Item.IsSettled);
    }

    /// <summary>
    /// Two people on the same project each need their own. That is the whole difference between a
    /// register and the single project level confirmation this replaces.
    /// </summary>
    [Fact]
    public async Task Handle_TwoAssignmentsEachCarryTheirOwnA1()
    {
        var first = WorkerScenario.Planned();
        var (a, _) = await Handle(
            first,
            ComplianceRequirement.A1Certificates,
            ComplianceStatus.Confirmed
        );
        first.Apply(a);

        var second = WorkerScenario.Planned();
        var (b, _) = await Handle(
            second,
            ComplianceRequirement.A1Certificates,
            ComplianceStatus.InProgress
        );
        second.Apply(b);

        Assert.True(first.Compliance[0].IsSettled);
        Assert.False(second.Compliance[0].IsSettled);
    }

    [Fact]
    public async Task Handle_ARequirementTheEngagementCarriesAsAWhole_BelongsToTheProject()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                WorkerScenario.Planned(EngagementType.TemporaryAgencyWork),
                ComplianceRequirement.DeAuegPermit,
                ComplianceStatus.Confirmed,
                reference: "AUG-1"
            )
        );

        Assert.Equal(
            RecordAssignmentComplianceItemHandler.BelongsToTheProjectMessage,
            error.Message
        );
    }

    /// <summary>
    /// The row that makes the engagement type worth having on the assignment. Employing somebody
    /// under German law is not a posting, so there is no A1 to ask for - and a system that asked
    /// would be chasing a document nobody will ever issue.
    /// </summary>
    [Fact]
    public async Task Handle_LocalEmployment_HasNoA1ToRecord()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                WorkerScenario.Planned(EngagementType.LocalEmployment),
                ComplianceRequirement.A1Certificates,
                ComplianceStatus.Confirmed
            )
        );

        Assert.Equal(RecordAssignmentComplianceItemHandler.NotInCatalogueMessage, error.Message);
    }

    [Fact]
    public async Task Handle_LocalEmployment_AsksForTheLocalContractAndFilingInstead()
    {
        var assignment = WorkerScenario.Planned(EngagementType.LocalEmployment);

        var (contract, _) = await Handle(
            assignment,
            ComplianceRequirement.LocalEmploymentContract,
            ComplianceStatus.Confirmed
        );
        assignment.Apply(contract);

        var (filing, _) = await Handle(
            assignment,
            ComplianceRequirement.DeSocialSecurityRegistration,
            ComplianceStatus.Confirmed,
            reference: "12345678"
        );
        assignment.Apply(filing);

        Assert.Equal(2, assignment.Compliance.Count);
    }

    [Fact]
    public async Task Handle_ARequirementFromAnotherCountry_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                WorkerScenario.Planned(workCountry: "BE"),
                ComplianceRequirement.DeLongTermPostingNotification,
                ComplianceStatus.Confirmed
            )
        );

        Assert.Equal(RecordAssignmentComplianceItemHandler.NotInCatalogueMessage, error.Message);
    }

    [Fact]
    public async Task Handle_ConfirmingANumberedRequirementWithoutItsNumber_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(
                WorkerScenario.Planned(workCountry: "BE"),
                ComplianceRequirement.BeLimosaDeclaration,
                ComplianceStatus.Confirmed
            )
        );

        Assert.Contains(
            RecordAssignmentComplianceItemHandler.ReferenceNumberRequiredMessage,
            error.Errors
        );
    }

    [Fact]
    public async Task Handle_PointingAtADocumentThatIsNotOnThisAssignment_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                WorkerScenario.Planned(),
                ComplianceRequirement.A1Certificates,
                ComplianceStatus.Confirmed,
                documentId: Guid.NewGuid()
            )
        );

        Assert.Equal(RecordAssignmentComplianceItemHandler.UnknownDocumentMessage, error.Message);
    }

    [Fact]
    public async Task Handle_RecordingTheSameRequirementTwice_Replaces()
    {
        var assignment = WorkerScenario.Planned();

        var (first, _) = await Handle(
            assignment,
            ComplianceRequirement.A1Certificates,
            ComplianceStatus.InProgress
        );
        assignment.Apply(first);

        var (second, _) = await Handle(
            assignment,
            ComplianceRequirement.A1Certificates,
            ComplianceStatus.Confirmed
        );
        assignment.Apply(second);

        Assert.Single(assignment.Compliance);
        Assert.True(assignment.Compliance[0].IsSettled);
    }

    private static Task<(AssignmentComplianceItemRecorded, Wolverine.Marten.Events)> Handle(
        Assignment assignment,
        ComplianceRequirement requirement,
        ComplianceStatus status,
        string? reference = null,
        DateOnly? validFrom = null,
        DateOnly? validTo = null,
        Guid? documentId = null
    ) =>
        RecordAssignmentComplianceItemHandler.Handle(
            new RecordAssignmentComplianceItem(
                WorkerScenario.AssignmentId,
                WorkerScenario.OrganizationId,
                requirement,
                status,
                reference,
                validFrom,
                validTo,
                documentId,
                null,
                WorkerScenario.UserId
            ),
            assignment,
            WorkerScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
