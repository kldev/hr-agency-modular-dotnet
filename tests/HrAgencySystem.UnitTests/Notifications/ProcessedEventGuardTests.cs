using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.NotificationWorker.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Notifications;

public sealed class ProcessedEventGuardTests
{
    private readonly IProcessedEventStore _store = Substitute.For<IProcessedEventStore>();

    private static readonly SendPasswordReset Message = new(
        Guid.NewGuid(),
        "identity",
        "Bob Smith",
        "bob.smith@hr-agency.com",
        "https://hr-agency.test/reset",
        15
    );

    [Fact]
    public async Task SendOnceAsync_WithUnclaimedEvent_Sends()
    {
        var sent = false;
        _store.TryMarkAsync(Message, Arg.Any<CancellationToken>()).Returns(true);

        await _store.SendOnceAsync(
            Message,
            NullLogger.Instance,
            () =>
            {
                sent = true;
                return Task.CompletedTask;
            },
            CancellationToken.None
        );

        Assert.True(sent);
        await _store.DidNotReceive().ReleaseAsync(Message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOnceAsync_WithEventClaimedByAnotherDelivery_DoesNotSend()
    {
        var sent = false;
        _store.TryMarkAsync(Message, Arg.Any<CancellationToken>()).Returns(false);

        await _store.SendOnceAsync(
            Message,
            NullLogger.Instance,
            () =>
            {
                sent = true;
                return Task.CompletedTask;
            },
            CancellationToken.None
        );

        Assert.False(sent);
    }

    [Fact]
    public async Task SendOnceAsync_WhenSendFails_ReleasesTheClaimSoTheRetryCanRun()
    {
        _store.TryMarkAsync(Message, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<TimeoutException>(() =>
            _store.SendOnceAsync(
                Message,
                NullLogger.Instance,
                () => throw new TimeoutException("smtp is down"),
                CancellationToken.None
            )
        );

        // Without this the next attempt would hit the guard, log "already handled" and report
        // success without ever sending the mail.
        await _store.Received(1).ReleaseAsync(Message, Arg.Any<CancellationToken>());
    }
}
