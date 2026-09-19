using HrAgencySystem.EmailTemplates.Contracts;

namespace HrAgencySystem.NotificationWorker.Infrastructure;

public interface IProcessedEventStore
{
    /// <summary>
    /// Claims the event for this delivery. False means another delivery already claimed it and the
    /// mail must not be sent again.
    /// </summary>
    Task<bool> TryMarkAsync(IEmailTemplateContract message, CancellationToken ct);
}
