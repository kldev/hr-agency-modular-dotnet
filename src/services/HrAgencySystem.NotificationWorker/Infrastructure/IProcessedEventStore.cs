using HrAgencySystem.EmailTemplates.Contracts;

namespace HrAgencySystem.NotificationWorker.Infrastructure;

public interface IProcessedEventStore
{
    /// <summary>
    /// Claims the event for this delivery. False means another delivery already claimed it and the
    /// mail must not be sent again.
    /// </summary>
    Task<bool> TryMarkAsync(IEmailTemplateContract message, CancellationToken ct);

    /// <summary>
    /// Gives the claim back after a failed send, so that the next attempt is allowed to try again.
    /// </summary>
    Task ReleaseAsync(IEmailTemplateContract message, CancellationToken ct);
}
