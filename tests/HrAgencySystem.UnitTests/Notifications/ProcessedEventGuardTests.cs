using HrAgencySystem.EmailTemplates.Contracts.Identity;
using System.Diagnostics.Metrics;
using HrAgencySystem.NotificationWorker.Infrastructure;
using HrAgencySystem.NotificationWorker.Infrastructure.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Notifications;

public sealed class ProcessedEventGuardTests : IDisposable
{
    private readonly IProcessedEventStore _store = Substitute.For<IProcessedEventStore>();
    private readonly ServiceProvider _services;
    private readonly NotificationMetrics _metrics;
    private readonly MetricCollector<long> _emails;

    public ProcessedEventGuardTests()
    {
        _services = new ServiceCollection().AddMetrics().BuildServiceProvider();
        var meters = _services.GetRequiredService<IMeterFactory>();
        _metrics = new NotificationMetrics(meters);
        _emails = new MetricCollector<long>(
            meters,
            NotificationMetrics.MeterName,
            "hr.notifications.emails"
        );
    }

    public void Dispose()
    {
        _emails.Dispose();
        _services.Dispose();
    }

    private void AssertSingleOutcome(string outcome)
    {
        var measurement = Assert.Single(_emails.GetMeasurementSnapshot());
        Assert.Equal(1, measurement.Value);
        Assert.Equal(outcome, measurement.Tags["outcome"]);
        Assert.Equal(nameof(SendPasswordReset), measurement.Tags["template"]);
    }

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
            _metrics,
            () =>
            {
                sent = true;
                return Task.CompletedTask;
            },
            CancellationToken.None
        );

        Assert.True(sent);
        await _store.DidNotReceive().ReleaseAsync(Message, Arg.Any<CancellationToken>());
        AssertSingleOutcome(NotificationMetrics.Sent);
    }

    [Fact]
    public async Task SendOnceAsync_WithEventClaimedByAnotherDelivery_DoesNotSend()
    {
        var sent = false;
        _store.TryMarkAsync(Message, Arg.Any<CancellationToken>()).Returns(false);

        await _store.SendOnceAsync(
            Message,
            NullLogger.Instance,
            _metrics,
            () =>
            {
                sent = true;
                return Task.CompletedTask;
            },
            CancellationToken.None
        );

        Assert.False(sent);
        AssertSingleOutcome(NotificationMetrics.Duplicate);
    }

    [Fact]
    public async Task SendOnceAsync_WhenSendFails_ReleasesTheClaimSoTheRetryCanRun()
    {
        _store.TryMarkAsync(Message, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<TimeoutException>(() =>
            _store.SendOnceAsync(
                Message,
                NullLogger.Instance,
                _metrics,
                () => throw new TimeoutException("smtp is down"),
                CancellationToken.None
            )
        );

        // Without this the next attempt would hit the guard, log "already handled" and report
        // success without ever sending the mail.
        await _store.Received(1).ReleaseAsync(Message, Arg.Any<CancellationToken>());
        AssertSingleOutcome(NotificationMetrics.Failed);
    }
}
