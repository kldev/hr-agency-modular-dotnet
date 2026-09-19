using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using Npgsql;
using Wolverine;
using Wolverine.ErrorHandling;

namespace HrAgencySystem.NotificationWorker.Infrastructure;

/// <summary>
/// What happens to a mail that did not make it out. Without these rules the first exception moves
/// the message straight to the dead letter queue, so a two second SMTP hiccup costs a notification.
/// </summary>
public static class EmailFailurePolicies
{
    /// Short and finite on purpose: a cooldown holds its listener, and three tries cover a restart
    /// of the SMTP host without letting one bad message block a whole queue for minutes.
    private static readonly TimeSpan[] Cooldowns =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15),
    ];

    extension(WolverineOptions opts)
    {
        /// <summary>
        /// Order matters: Wolverine takes the first rule whose exception type matches, so the
        /// failures that can never succeed are declared before the ones that are only a broken
        /// connection away from working.
        /// </summary>
        public void RetryEmailDelivery()
        {
            // The message's own fault - a malformed recipient address or a permanent 5xx refusal.
            // Every retry ends exactly the same way, so it goes to the dead letter queue at once.
            opts.Policies.OnException<ParseException>().MoveToErrorQueue();
            opts.Policies.OnException<SmtpCommandException>(e => (int)e.StatusCode >= 500)
                .MoveToErrorQueue();

            // Everything below is the infrastructure having a bad minute: the SMTP host restarting,
            // a dropped connection, a Postgres failover - the same mail sent again simply works.
            opts.Policies.OnException<SmtpCommandException>().RetryWithCooldown(Cooldowns);
            opts.Policies.OnException<SmtpProtocolException>().RetryWithCooldown(Cooldowns);
            opts.Policies.OnException<ServiceNotConnectedException>().RetryWithCooldown(Cooldowns);
            opts.Policies.OnException<ServiceNotAuthenticatedException>()
                .RetryWithCooldown(Cooldowns);
            opts.Policies.OnException<SocketException>().RetryWithCooldown(Cooldowns);
            opts.Policies.OnException<IOException>().RetryWithCooldown(Cooldowns);
            opts.Policies.OnException<TimeoutException>().RetryWithCooldown(Cooldowns);
            opts.Policies.OnException<NpgsqlException>().RetryWithCooldown(Cooldowns);
        }
    }
}
