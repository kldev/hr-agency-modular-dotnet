using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.WorkAuthorisations.Record;
using HrAgencySystem.Workers.Application.WorkerDocuments.Remove;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;

namespace HrAgencySystem.UnitTests.Workers;

/// <summary>
/// Permits belong to the person, not to any posting: a residence card does not expire because a
/// project ended, and the next project does not need a new one. That is exactly what separates it
/// from an A1.
/// </summary>
public class WorkAuthorisationTests : BaseTest
{
    [Fact]
    public async Task Handle_RecordsAPermitForAThirdCountryNational()
    {
        var worker = WorkerScenario.Registered(WorkerScenario.UkrainianCitizenship);

        var (result, _) = await Handle(worker);

        Assert.Equal(WorkAuthorisationKind.WorkPermit, result.Authorisation.Kind);
        Assert.Equal("PL", result.Authorisation.Country);
        Assert.True(result.Authorisation.IsValidOn(new DateOnly(2027, 1, 1)));
    }

    [Fact]
    public async Task Handle_ForSomebodyWithFreeMovement_Refuses()
    {
        // Not a harmless extra row: it would claim this person's right to work depends on a piece
        // of paper, and the next reader would act on it.
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            // ReSharper disable once RedundantArgumentDefaultValue
            Handle(WorkerScenario.Registered(WorkerScenario.PolishCitizenship))
        );

        Assert.Equal(RecordWorkAuthorisationHandler.NotRequiredMessage, error.Message);
    }

    [Fact]
    public async Task Handle_RenewingUnderTheSameIdReplacesIt()
    {
        var worker = WorkerScenario.Registered(WorkerScenario.UkrainianCitizenship);

        var (first, _) = await Handle(worker);
        worker.Apply(first);

        var (renewed, _) = await Handle(
            worker,
            authorisationId: first.Authorisation.AuthorisationId,
            validUntil: new DateOnly(2029, 12, 31)
        );
        worker.Apply(renewed);

        Assert.Single(worker.Authorisations);
        Assert.Equal(new DateOnly(2029, 12, 31), worker.Authorisations[0].ValidUntil);
    }

    [Fact]
    public async Task Handle_ExpiringBeforeItStarts_ThrowsValidation()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Handle(
                WorkerScenario.Registered(WorkerScenario.UkrainianCitizenship),
                validFrom: new DateOnly(2027, 1, 1),
                validUntil: new DateOnly(2026, 1, 1)
            )
        );

        Assert.Contains(WorkAuthorisation.ValidUntilBeforeValidFromMessage, error.Errors);
    }

    [Fact]
    public async Task Handle_PointingAtADocumentThatIsNotOnTheFile_Refuses()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Handle(
                WorkerScenario.Registered(WorkerScenario.UkrainianCitizenship),
                documentId: Guid.NewGuid()
            )
        );

        Assert.Equal(RecordWorkAuthorisationHandler.UnknownDocumentMessage, error.Message);
    }

    [Fact]
    public async Task RemovingTheScanBehindAPermitIsRefused()
    {
        // A permission claiming evidence that is no longer there is the one state this file must
        // never be able to reach.
        var documentId = Guid.NewGuid();
        var worker = WorkerScenario
            .Registered(WorkerScenario.UkrainianCitizenship)
            .WithDocument(documentId);

        var (recorded, _) = await Handle(worker, documentId: documentId);
        worker.Apply(recorded);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            RemoveWorkerDocumentHandler.Handle(
                new RemoveWorkerDocument(
                    WorkerScenario.WorkerId,
                    WorkerScenario.OrganizationId,
                    documentId,
                    WorkerScenario.UserId
                ),
                worker,
                WorkerScenario.Service(),
                new FixedClock(DateTimeOffset.UtcNow),
                CancellationToken.None
            )
        );

        Assert.Equal(RemoveWorkerDocumentHandler.ReferencedByAuthorisationMessage, error.Message);
    }

    private static Task<(WorkAuthorisationRecorded, Wolverine.Marten.Events)> Handle(
        Worker worker,
        Guid? authorisationId = null,
        Guid? documentId = null,
        DateOnly? validFrom = null,
        DateOnly? validUntil = null
    ) =>
        RecordWorkAuthorisationHandler.Handle(
            new RecordWorkAuthorisation(
                WorkerScenario.WorkerId,
                WorkerScenario.OrganizationId,
                authorisationId,
                WorkAuthorisationKind.WorkPermit,
                "PL",
                "ZEZW/2026/123",
                validFrom ?? new DateOnly(2026, 1, 1),
                validUntil ?? new DateOnly(2027, 12, 31),
                documentId,
                null,
                WorkerScenario.UserId
            ),
            worker,
            WorkerScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
