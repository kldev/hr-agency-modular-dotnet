using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Application.Handlers;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using D = HrAgencySystem.JobDescription.Domain;
using NSubstitute;
using Wolverine.Marten;

namespace HrAgencySystem.UnitTests.JobDescription.Handlers;

public sealed class AssignRecruiterJobDescriptionHandlerTests
{
    private readonly IUserSnapshotService _snapshotService = Substitute.For<IUserSnapshotService>();
    private readonly IClock _clock = Substitute.For<IClock>();
    
    [Fact]
    public async Task Should_assign_recruiter()
    {
        // Arrange
        var jobDescriptionId = Guid.NewGuid();
        var recruiterId = Guid.NewGuid();
        var modifiedId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        
        var now = new DateTimeOffset(2026, 9, 2, 10, 0, 0, TimeSpan.Zero);

        var command = new AssignRecruiterJobDescription(
            jobDescriptionId,
            recruiterId, modifiedId, organizationId);

        var aggregate = D.JobDescription.EmptyWithOrganizationId(new OrganizationId(organizationId));
        ;

        var recruiter = new UserSnapshot(
            recruiterId,
            "Anna",
            "Kowalska",
            "anna.kowalska@example.com");
        
        var modifyBy = new UserSnapshot(
            recruiterId,
            "Greg",
            "Loon",
            "greg.loon@example.com");

        _snapshotService
            .GetUserAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns(recruiter);
        
        _snapshotService
            .GetUserAsync(modifiedId, Arg.Any<CancellationToken>())
            .Returns(modifyBy);

        _clock.UtcNow.Returns(now);

        // Act
        var (result, events) = await AssignRecruiterJobDescriptionHandler.Handle(
            command,
            aggregate,
            _snapshotService,
            _clock,
            CancellationToken.None);

        // Assert
        Assert.IsType<JobDescriptionRecruiterAssigned>(result);
        Assert.Equal(recruiter, result.Recruiter);
        Assert.Equal(now, result.OccurredAt);

        Assert.Single(events);
        Assert.Single(events, result);
    }

    [Fact]
    public async Task Should_throw_not_found_when_job_description_does_not_exist()
    {
        var modifiedId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        // Arrange
        var command = new AssignRecruiterJobDescription(
            Guid.NewGuid(),
            Guid.NewGuid(), modifiedId, organizationId);
        
        // Assert
        var exception = await Assert
            .ThrowsAsync<NotFoundException>( async () => await AssignRecruiterJobDescriptionHandler.Handle(
                command,
                null!,
                _snapshotService,
                _clock,
                CancellationToken.None));
        
        Assert.Contains("Not found", exception.Message);

        await _snapshotService
            .DidNotReceive()
            .GetUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_throw_business_rule_exception_when_recruiter_does_not_exist()
    {
        // Arrange
        var modifiedId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var recruiterId = Guid.NewGuid();

        var command = new AssignRecruiterJobDescription(
            Guid.NewGuid(),
            recruiterId, modifiedId, organizationId);

        _snapshotService
            .GetUserAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        // Act
        Task<(JobDescriptionRecruiterAssigned, Events)> Act() =>
            AssignRecruiterJobDescriptionHandler.Handle(command, D.JobDescription.Empty(), _snapshotService, _clock,
                CancellationToken.None);

        // Assert
        var exception = await Assert
            .ThrowsAsync<BusinessRuleException>(async () => await Act());

        Assert.Equal(IUserSnapshotService.NotFoundMessage, exception.Message);
    }
}