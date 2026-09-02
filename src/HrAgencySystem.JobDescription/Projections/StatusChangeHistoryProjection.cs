using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Marten.Patching;

namespace HrAgencySystem.JobDescription.Projections;

public sealed class StatusChangeHistoryProjection : IProjection
{
    private void Project(
        IDocumentOperations operations,
        JobDescriptionCreated @event)
    {
        operations.Store(
            new StatusChangeHistoryProjectionDocument
            {
                Id = @event.JobDescriptionId,
                JobDescriptionId = @event.JobDescriptionId,
                CurrentStatus = JobDescriptionStatus.Draft,
                CurrentStatusStartedAt = @event.CreatedAt,
                OrganizationId = @event.OrganizationId
            });
    }

    private async Task Project(
        IDocumentOperations operations,
        JobDescriptionOpened @event, CancellationToken ct)
    {
        await ChangeStatus(
            operations,
            @event.JobDescriptionId,
            JobDescriptionStatus.Open,
            @event.OccurredAt, ct);
    }

    private async Task Project(
        IDocumentOperations operations,
        JobDescriptionPutOnHold @event, 
        CancellationToken ct)
    {
        await ChangeStatus(
            operations,
            @event.JobDescriptionId,
            JobDescriptionStatus.OnHold,
            @event.OccurredAt, ct);
    }

    private async Task Project(
        IDocumentOperations operations,
        JobDescriptionClosed @event, CancellationToken ct)
    {
        await ChangeStatus(
            operations,
            @event.JobDescriptionId,
            JobDescriptionStatus.Closed,
            @event.OccurredAt, ct);
    }

    private async Task Project(
        IDocumentOperations operations,
        JobDescriptionCancelled @event,  CancellationToken ct)
    {
        await ChangeStatus(
            operations,
            @event.JobDescriptionId,
            JobDescriptionStatus.Cancelled,
            @event.OccurredAt, ct);
    }

    private static async Task ChangeStatus(
        IDocumentOperations operations,
        Guid jobDescriptionId,
        JobDescriptionStatus newStatus,
        DateTimeOffset changedAt, CancellationToken ct)
    {

        var document = await operations.Query<StatusChangeHistoryProjectionDocument>()
            .Where(z => z.Id == jobDescriptionId)
            .FirstOrDefaultAsync(ct);

        if (document is null)
        {
            throw new InvalidOperationException(
                $"Status history not found for job description {jobDescriptionId}.");
        }

        var timeInPreviousStatus =
            changedAt - document.CurrentStatusStartedAt;

        document.Changes.Add(
            new StatusChange(
                document.CurrentStatus,
                newStatus,
                changedAt,
                timeInPreviousStatus));

        document.CurrentStatus = newStatus;
        document.CurrentStatusStartedAt = changedAt;

        operations.Patch<StatusChangeHistoryProjectionDocument>(jobDescriptionId).Set(z => z.Changes, document.Changes);
        operations.Patch<StatusChangeHistoryProjectionDocument>(jobDescriptionId).Set(z => z.CurrentStatus, newStatus);
        operations.Patch<StatusChangeHistoryProjectionDocument>(jobDescriptionId).Set(z => z.CurrentStatusStartedAt, changedAt);
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        foreach (var @event in events)
        {
            switch (@event.Data)
            {
                case JobDescriptionCreated created: Project(operations, created);
                    break;
                case JobDescriptionClosed closed:
                    await Project(operations, closed, cancellation);
                    break;
                case JobDescriptionCancelled cancelled:
                    await Project(operations, cancelled, cancellation);
                    break;
                case JobDescriptionPutOnHold  putOnHold:
                    await Project(operations, putOnHold, cancellation);
                    break;
                case JobDescriptionOpened opened:
                    await Project(operations, opened, cancellation);
                    break;
            }
        }
    }
}

public sealed class StatusChangeHistoryProjectionDocument
{
    public Guid Id { get; init; }

    public Guid JobDescriptionId { get; init; }
    
    public Guid OrganizationId { get; init; }

    public JobDescriptionStatus CurrentStatus { get; set; }

    public DateTimeOffset CurrentStatusStartedAt { get; set; }

    public List<StatusChange> Changes { get; set; } = [];
}

public sealed record StatusChange(
    JobDescriptionStatus PreviousStatus,
    JobDescriptionStatus NewStatus,
    DateTimeOffset ChangedAt,
    TimeSpan TimeInPreviousStatus);