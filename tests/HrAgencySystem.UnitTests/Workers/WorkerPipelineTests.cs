using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.ChangeWorkerStatus;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.UnitTests.Workers;

/// <summary>
/// The pipeline and the one branch in it. Which stages a person goes through is decided by their
/// citizenship and by nothing else - there is no country named anywhere in a handler.
/// </summary>
public class WorkerPipelineTests : BaseTest
{
    [Fact]
    public void AnEeaNationalSkipsLegalisationEntirely()
    {
        Assert.False(LegalisationPolicy.RequiresLegalisation(WorkerScenario.PolishCitizenship));

        // Straight from the contract to being put to work.
        Assert.True(
            WorkerStatusChangePolicy.Allow(
                WorkerStatus.ContractPreparation,
                WorkerStatus.Onboarding,
                requiresLegalisation: false
            )
        );
        Assert.False(
            WorkerStatusChangePolicy.Allow(
                WorkerStatus.ContractPreparation,
                WorkerStatus.Legalisation,
                requiresLegalisation: false
            )
        );
    }

    [Fact]
    public void AThirdCountryNationalGoesThroughLegalisationFirst()
    {
        Assert.True(LegalisationPolicy.RequiresLegalisation(WorkerScenario.UkrainianCitizenship));

        Assert.True(
            WorkerStatusChangePolicy.Allow(
                WorkerStatus.ContractPreparation,
                WorkerStatus.Legalisation,
                requiresLegalisation: true
            )
        );

        // And cannot go round it: the residence card and the permit are not optional.
        Assert.False(
            WorkerStatusChangePolicy.Allow(
                WorkerStatus.ContractPreparation,
                WorkerStatus.Onboarding,
                requiresLegalisation: true
            )
        );
    }

    [Theory]
    [InlineData(WorkerStatus.Recruitment, ResponsibleDepartment.Recruitment)]
    [InlineData(WorkerStatus.ContractPreparation, ResponsibleDepartment.HumanResources)]
    [InlineData(WorkerStatus.Legalisation, ResponsibleDepartment.Legalisation)]
    [InlineData(WorkerStatus.Onboarding, ResponsibleDepartment.Operations)]
    [InlineData(WorkerStatus.ProjectChange, ResponsibleDepartment.Operations)]
    [InlineData(WorkerStatus.Terminated, ResponsibleDepartment.None)]
    public void EachStageHasADepartmentThatOwnsIt(
        WorkerStatus status,
        ResponsibleDepartment expected
    ) => Assert.Equal(expected, WorkerStatusChangePolicy.OwnerOf(status));

    [Fact]
    public async Task Handle_WalksAPolishWorkerToEmployed()
    {
        var worker = WorkerScenario.Registered();

        var (contract, _) = await Handle(worker, WorkerStatus.ContractPreparation);
        worker.Apply(contract);

        var (onboarding, _) = await Handle(worker, WorkerStatus.Onboarding);
        worker.Apply(onboarding);

        var (employed, _) = await Handle(worker, WorkerStatus.Employed);
        worker.Apply(employed);

        Assert.Equal(WorkerStatus.Employed, worker.Status);
        Assert.Equal(WorkerStatus.Onboarding, employed.PreviousStatus);
    }

    [Fact]
    public async Task Handle_SendingAnEeaNationalToLegalisation_SaysWhyNot()
    {
        var worker = WorkerScenario.Registered().InStatus(WorkerStatus.ContractPreparation);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(worker, WorkerStatus.Legalisation)
        );

        // Not "this change is not allowed": the reason is that they do not need it.
        Assert.Equal(ChangeWorkerStatusHandler.LegalisationNotRequiredMessage, error.Message);
    }

    [Fact]
    public async Task Handle_SkippingAStage_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(WorkerScenario.Registered(), WorkerStatus.Employed)
        );

        Assert.Equal(ChangeWorkerStatusHandler.TransitionNotAllowedMessage, error.Message);
    }

    [Fact]
    public async Task Handle_WithAnExpiredIdentityDocument_DoesNotLetSomebodyBecomeEmployed()
    {
        var worker = WorkerScenario
            .Registered(documentValidUntil: WorkerScenario.Today.AddDays(-1))
            .InStatus(WorkerStatus.ContractPreparation)
            .InStatus(WorkerStatus.Onboarding);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(worker, WorkerStatus.Employed)
        );

        Assert.Equal(ChangeWorkerStatusHandler.IdentityDocumentExpiredMessage, error.Message);
    }

    [Fact]
    public async Task Handle_MovingSomebodyToAnotherProjectIsItsOwnStage()
    {
        // Their papers are in order; what the move needs is new posting paperwork, and somebody has
        // to see that on a list.
        var worker = WorkerScenario.Registered().Employed();

        var (moving, _) = await Handle(worker, WorkerStatus.ProjectChange);
        worker.Apply(moving);

        Assert.Equal(
            ResponsibleDepartment.Operations,
            WorkerStatusChangePolicy.OwnerOf(worker.Status)
        );

        var (back, _) = await Handle(worker, WorkerStatus.Employed);
        worker.Apply(back);

        Assert.Equal(WorkerStatus.Employed, worker.Status);
    }

    [Fact]
    public async Task Handle_SomebodyWhoLeftCanBeRehiredWithoutASecondFile()
    {
        var worker = WorkerScenario.Registered().Employed();

        var (left, _) = await Handle(worker, WorkerStatus.Terminated, "End of contract");
        worker.Apply(left);

        // Back in at the contract, not at recruitment: the same person, the same history.
        var (rehired, _) = await Handle(worker, WorkerStatus.ContractPreparation);
        worker.Apply(rehired);

        Assert.Equal(WorkerStatus.ContractPreparation, worker.Status);
        Assert.Equal(WorkerScenario.WorkerId, worker.Id.Value);
    }

    private static Task<(WorkerStatusChanged, Wolverine.Marten.Events)> Handle(
        Worker worker,
        WorkerStatus status,
        string? reason = null
    ) =>
        ChangeWorkerStatusHandler.Handle(
            new ChangeWorkerStatus(
                WorkerScenario.WorkerId,
                WorkerScenario.OrganizationId,
                status,
                reason,
                WorkerScenario.UserId
            ),
            worker,
            WorkerScenario.Service(),
            new FixedClock(
                new DateTimeOffset(
                    WorkerScenario.Today.ToDateTime(TimeOnly.MinValue),
                    TimeSpan.Zero
                )
            ),
            CancellationToken.None
        );
}
