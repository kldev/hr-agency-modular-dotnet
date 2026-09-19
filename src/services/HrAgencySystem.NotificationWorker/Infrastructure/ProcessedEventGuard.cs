using HrAgencySystem.EmailTemplates.Contracts;

namespace HrAgencySystem.NotificationWorker.Infrastructure;

public static class ProcessedEventGuard
{
    extension(IProcessedEventStore store)
    {
        /// <summary>
        /// Sends a mail once per event id. The claim is released when the send throws, because a
        /// mark left behind would turn every retry and every replay into a silent no-op - the
        /// handler would report success without a mail ever leaving.
        /// </summary>
        public async Task SendOnceAsync(
            IEmailTemplateContract message,
            ILogger logger,
            Func<Task> send,
            CancellationToken ct
        )
        {
            if (!await store.TryMarkAsync(message, ct))
            {
                logger.LogInformation("Event {EventId} already handled, skipping", message.EventId);
                return;
            }

            try
            {
                await send();
            }
            catch
            {
                // Not ct: a cancelled delivery still has to give the claim back.
                await store.ReleaseAsync(message, CancellationToken.None);
                throw;
            }
        }
    }
}
