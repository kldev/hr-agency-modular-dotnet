using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Marten.Patching;

namespace HrAgencySystem.JobDescription.Projections;

public partial class StatusChangeHistoryProjection : EventProjection
{
    // ReSharper disable once MemberCanBePrivate.Global
    public JdStatusChangeHistory Create(
        IEvent<JobDescriptionCreated> @event) =>
        new()
        {
            Id = @event.Data.JobDescriptionId,
            JobDescriptionId = @event.Data.JobDescriptionId,
            CurrentStatus = JobDescriptionStatus.Draft,
            CurrentStatusStartedAt = @event.Data.CreatedAt,
            OrgId = @event.Data.OrganizationId
        };



    // ReSharper disable once MemberCanBePrivate.Global
    public async Task Project(
        IEvent<JobDescriptionOpened> @event, IDocumentOperations ops, CancellationToken ct)
    {
        await ChangeStatus(
            ops,
            @event.Data.JobDescriptionId,
            JobDescriptionStatus.Open,
            @event.Data.OccurredAt, ct);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task Project(
        IEvent<JobDescriptionPutOnHold> @event,
        IDocumentOperations operations,
        CancellationToken ct)
    {
        await ChangeStatus(
            operations,
            @event.Data.JobDescriptionId,
            JobDescriptionStatus.OnHold,
            @event.Data.OccurredAt, ct);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task Project(
        IEvent<JobDescriptionClosed> @event, 
        IDocumentOperations operations,
        CancellationToken ct)
    {
        await ChangeStatus(
            operations,
            @event.Data.JobDescriptionId,
            JobDescriptionStatus.Closed,
            @event.Data.OccurredAt, ct);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task Project(
        IEvent<JobDescriptionCancelled> @event,  
        IDocumentOperations operations,CancellationToken ct)
    {
        await ChangeStatus(
            operations,
            @event.Data.JobDescriptionId,
            JobDescriptionStatus.Cancelled,
            @event.Data.OccurredAt, ct);
    }

    private static async Task ChangeStatus(
        IDocumentOperations operations,
        Guid jobDescriptionId,
        JobDescriptionStatus newStatus,
        DateTimeOffset changedAt, CancellationToken ct)
    {

        var document = await operations.LoadAsync<JdStatusChangeHistory>(jobDescriptionId, ct);
        
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

        operations.Patch<JdStatusChangeHistory>(jobDescriptionId).Set(z => z.Changes, document.Changes);
        operations.Patch<JdStatusChangeHistory>(jobDescriptionId).Set(z => z.CurrentStatus, newStatus);
        operations.Patch<JdStatusChangeHistory>(jobDescriptionId).Set(z => z.CurrentStatusStartedAt, changedAt);
    }
}

public sealed class JdStatusChangeHistory
{
    public Guid Id { get; init; }

    public Guid JobDescriptionId { get; init; }
    
    public Guid OrgId { get; init; }

    public JobDescriptionStatus CurrentStatus { get; set; }

    public DateTimeOffset CurrentStatusStartedAt { get; set; }

    public List<StatusChange> Changes { get; set; } = [];
}

public sealed record StatusChange(
    JobDescriptionStatus PreviousStatus,
    JobDescriptionStatus NewStatus,
    DateTimeOffset ChangedAt,
    TimeSpan TimeInPreviousStatus);