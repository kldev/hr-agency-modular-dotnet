using HrAgencySystem.JobDescription.Application.ChangeStatus;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using NSubstitute;
using D = HrAgencySystem.JobDescription.Domain;

namespace HrAgencySystem.UnitTests.JobDescriptions.Handlers;

public sealed class ChangeJobDescriptionStatusHandlerTests
{
    private readonly IClock _clock = Substitute.For<IClock>();

    private readonly IJobDescriptionService _service =
        Substitute.For<IJobDescriptionService>();

    private UserSnapshot ModifiedBy { get; } = new (Guid.NewGuid(), "Test", "User", "test@test.io");
    
    [Fact]
    public async Task Should_close_job_description()
    {
        // Arrange
        var jobDescriptionId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);

        var command = new ChangeJobDescriptionStatus(
            jobDescriptionId,
            D.JobDescriptionStatus.Closed,
            Guid.NewGuid(), Guid.NewGuid());

        var aggregate = D.JobDescription.Empty();

        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);

        _clock.UtcNow.Returns(now);

        // Act
        var (result, events) = await ChangeJobDescriptionStatusHandler.Handle(
            command,
            aggregate,
            _service,
            _clock,
            CancellationToken.None);

        // Assert
        Assert.Equal(aggregate.Id.Value, result.JobDescriptionId);
        Assert.Equal(D.JobDescriptionStatus.Closed, result.Status);

        var @event = Assert.Single(events);
        var closedEvent = Assert.IsType<JobDescriptionClosed>(@event);

        Assert.Equal(now, closedEvent.OccurredAt);
    }

    [Fact]
    public async Task Should_cancel_job_description()
    {
        // Arrange
        var jobDescriptionId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);

        var command = new ChangeJobDescriptionStatus(
            jobDescriptionId,
            D.JobDescriptionStatus.Cancelled,
            Guid.NewGuid(), Guid.NewGuid());

        var aggregate = D.JobDescription.Empty();

        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);
        
        _clock.UtcNow.Returns(now);

        // Act
        var (result, events) = await ChangeJobDescriptionStatusHandler.Handle(
            command,
            aggregate,
            _service,
            _clock, CancellationToken.None);

        // Assert
        Assert.Equal(aggregate.Id.Value, result.JobDescriptionId);
        Assert.Equal(D.JobDescriptionStatus.Cancelled, result.Status);

        var @event = Assert.Single(events);
        var cancelledEvent = Assert.IsType<JobDescriptionCancelled>(@event);

        Assert.Equal(now, cancelledEvent.OccurredAt);
    }

    [Fact]
    public async Task Should_put_job_description_on_hold()
    {
        // Arrange
        var jobDescriptionId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);

        var command = new ChangeJobDescriptionStatus(
            jobDescriptionId,
            D.JobDescriptionStatus.OnHold,
            Guid.NewGuid(), Guid.NewGuid());

        var aggregate = D.JobDescription.Empty();
        
        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);

        _clock.UtcNow.Returns(now);

        // Act
        var (result, events) = await ChangeJobDescriptionStatusHandler.Handle(
            command,
            aggregate,
            _service,
            _clock,
            CancellationToken.None);

        // Assert
        Assert.Equal(aggregate.Id.Value, result.JobDescriptionId);
        Assert.Equal(D.JobDescriptionStatus.OnHold, result.Status);

        var @event = Assert.Single(events);
        var holdEvent = Assert.IsType<JobDescriptionPutOnHold>(@event);

        Assert.Equal(now, holdEvent.OccurredAt);
    }

    [Fact]
    public async Task Should_open_job_description()
    {
        // Arrange
        var jobDescriptionId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);

        var command = new ChangeJobDescriptionStatus(
            jobDescriptionId,
            D.JobDescriptionStatus.Open,
            Guid.NewGuid(), Guid.NewGuid());

        var aggregate = D.JobDescription.Empty();
        
        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);

        _clock.UtcNow.Returns(now);

        // Act
        var (result, events) = await ChangeJobDescriptionStatusHandler.Handle(
            command,
            aggregate,
            _service,
            _clock, CancellationToken.None);

        // Assert
        Assert.Equal(aggregate.Id.Value, result.JobDescriptionId);
        Assert.Equal(D.JobDescriptionStatus.Open, result.Status);

        Assert.Single(events);
        Assert.IsType<JobDescriptionOpened>(events[0]);

        var openEvent = Assert.IsType<JobDescriptionOpened>(events[0]);
        Assert.Equal(now, openEvent.OccurredAt);
    }

    [Fact]
    public async Task Should_not_create_event_when_status_is_not_changed()
    {
        // Arrange
        var aggregate = D.JobDescription.Empty();

        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);
        
        var command = new ChangeJobDescriptionStatus(
            aggregate.Id.Value,
            aggregate.Status,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var (result, events) = await ChangeJobDescriptionStatusHandler.Handle(
            command,
            aggregate,
            _service,
            _clock,
            CancellationToken.None);

        // Assert
        Assert.Equal(aggregate.Id.Value, result.JobDescriptionId);
        Assert.Equal(aggregate.Status, result.Status);

        Assert.Empty(events);
    }

    [Fact]
    public async Task Should_throw_not_found_when_job_description_does_not_exist()
    {
        // Arrange
        var jobDescriptionId = Guid.NewGuid();

        var command = new ChangeJobDescriptionStatus(
            jobDescriptionId,
            D.JobDescriptionStatus.Closed,
            Guid.NewGuid(), Guid.NewGuid());

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await ChangeJobDescriptionStatusHandler.Handle(
                command,
                null!,
                _service,
                _clock, CancellationToken.None));

        // Assert
        Assert.Contains("not found", exception.Message);
        Assert.Contains(jobDescriptionId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Should_throw_business_rule_exception_when_status_is_invalid()
    {
        // Arrange
        var aggregate = D.JobDescription.Empty();

        var invalidStatus = (D.JobDescriptionStatus)999;

        var command = new ChangeJobDescriptionStatus(
            aggregate.Id.Value,
            invalidStatus, Guid.NewGuid(), Guid.NewGuid());

        _service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);
        
        // Act
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            async () => await ChangeJobDescriptionStatusHandler.Handle(
                command,
                aggregate,
                _service,
                _clock, CancellationToken.None));

        // Assert
        Assert.Equal(
            "Invalid status change: " + invalidStatus,
            exception.Message);
    }
}