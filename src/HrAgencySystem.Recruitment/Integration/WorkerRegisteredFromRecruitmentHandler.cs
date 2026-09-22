using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace HrAgencySystem.Recruitment.Integration;

/// <summary>
/// A workers' file was opened from recruitment. The candidate learns that the person is in the
/// register, and the application - when there was one - that it is the one it came out of.
/// <para>
/// <c>Workers</c> cannot see either record, so the ids arrive as claims and are checked here. A
/// claim that does not hold is <b>skipped with a warning, never thrown</b>: a message that does not
/// apply will not apply any better on a retry, and a dead letter helps nobody. The same goes for a
/// repeat, since these arrive at least once.
/// </para>
/// </summary>
[WolverineHandler]
public static class WorkerRegisteredFromRecruitmentHandler
{
    public static async Task Handle(
        WorkerRegisteredFromRecruitment message,
        IDocumentSession session,
        IClock clock,
        ILogger<WorkerRegisteredFromRecruitment> logger,
        CancellationToken ct
    )
    {
        var candidate = await session.Events.AggregateStreamAsync<Candidate>(
            message.CandidateId,
            token: ct
        );

        if (candidate is null || candidate.OrganizationId.Value != message.OrganizationId)
        {
            logger.LogWarning(
                "Worker {WorkerId} names candidate {CandidateId}, which is not in organization {OrganizationId}; nothing recorded",
                message.WorkerId,
                message.CandidateId,
                message.OrganizationId
            );

            return;
        }

        MarkCandidate(message, candidate, session, clock, logger);

        if (message.ApplicationId is { } applicationId)
            await MarkApplication(message, applicationId, session, clock, logger, ct);
    }

    /// <summary>
    /// One person, one file is guarded on the workers' side, so a second worker for the same
    /// candidate means something went wrong over there. The first one stays: it is the file the
    /// person's history already hangs on.
    /// </summary>
    private static void MarkCandidate(
        WorkerRegisteredFromRecruitment message,
        Candidate candidate,
        IDocumentSession session,
        IClock clock,
        ILogger logger
    )
    {
        if (candidate.WorkerId == message.WorkerId)
            return;

        if (candidate.WorkerId is { } existing)
        {
            logger.LogWarning(
                "Candidate {CandidateId} already has worker {ExistingWorkerId}; ignoring worker {WorkerId}",
                message.CandidateId,
                existing,
                message.WorkerId
            );

            return;
        }

        session.Events.Append(
            message.CandidateId,
            new CandidateRegisteredAsWorker(message.CandidateId, message.WorkerId, clock.UtcNow)
        );
    }

    private static async Task MarkApplication(
        WorkerRegisteredFromRecruitment message,
        Guid applicationId,
        IDocumentSession session,
        IClock clock,
        ILogger logger,
        CancellationToken ct
    )
    {
        var application = await session.Events.AggregateStreamAsync<JobApplication>(
            applicationId,
            token: ct
        );

        // The candidate check matters as much as the organization one: an application of
        // somebody else would otherwise say it produced this person's file.
        if (
            application is null
            || application.OrganizationId.Value != message.OrganizationId
            || application.CandidateId.Value != message.CandidateId
        )
        {
            logger.LogWarning(
                "Worker {WorkerId} names application {ApplicationId}, which is not candidate {CandidateId}'s in organization {OrganizationId}; application left as it is",
                message.WorkerId,
                applicationId,
                message.CandidateId,
                message.OrganizationId
            );

            return;
        }

        if (application.WorkerId is not null)
            return;

        session.Events.Append(
            applicationId,
            new JobApplicationRegisteredAsWorker(applicationId, message.WorkerId, clock.UtcNow)
        );
    }
}
