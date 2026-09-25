using System.Diagnostics;
using HrAgencySystem.EmailTemplates.Contracts;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;

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
            NotificationMetrics metrics,
            Func<Task> send,
            CancellationToken ct
        )
        {
            var template = message.GetType().Name;

            if (!await store.TryMarkAsync(message, ct))
            {
                logger.LogInformation("Event {EventId} already handled, skipping", message.EventId);
                metrics.Record(template, NotificationMetrics.Duplicate);
                return;
            }

            var started = Stopwatch.GetTimestamp();
            try
            {
                await send();
                metrics.Record(template, NotificationMetrics.Sent);
            }
            catch
            {
                metrics.Record(template, NotificationMetrics.Failed);
                // Not ct: a cancelled delivery still has to give the claim back.
                await store.ReleaseAsync(message, CancellationToken.None);
                throw;
            }
            finally
            {
                metrics.RecordDuration(template, Stopwatch.GetElapsedTime(started));
            }
        }
    }
}
