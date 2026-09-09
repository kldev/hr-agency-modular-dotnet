using HrAgencySystem.JobDescription.Application.AssignRecruiter;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Wolverine.Marten;
using D = HrAgencySystem.JobDescription.Domain;

namespace HrAgencySystem.UnitTests.JobDescriptions.Handlers;

public sealed class AssignJobDescriptionRecruiterHandlerTests
{
    private readonly IJobDescriptionService _service = Substitute.For<IJobDescriptionService>();
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

        var command = new AssignJobDescriptionRecruiter(
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

        _service
            .GetUserAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns(recruiter);
        
        _service
            .GetUserAsync(modifiedId, Arg.Any<CancellationToken>())
            .Returns(modifyBy);

        _clock.UtcNow.Returns(now);

        // Act
        var (result, events) = await AssignJobDescriptionRecruiterHandler.Handle(
            command,
            aggregate,
            _service,
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
        var command = new AssignJobDescriptionRecruiter(
            Guid.NewGuid(),
            Guid.NewGuid(), modifiedId, organizationId);
        
        // Assert
        var exception = await Assert
            .ThrowsAsync<NotFoundException>( async () => await AssignJobDescriptionRecruiterHandler.Handle(
                command,
                null!,
                _service,
                _clock,
                CancellationToken.None));
        
        Assert.Contains("not found", exception.Message);

        await _service
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

        var command = new AssignJobDescriptionRecruiter(
            Guid.NewGuid(),
            recruiterId, modifiedId, organizationId);

        _service
            .GetUserAsync(recruiterId, Arg.Any<CancellationToken>())
            .Throws(new NotFoundException("User", recruiterId));

        // Act
        Task<(JobDescriptionRecruiterAssigned, Events)> Act() =>
            AssignJobDescriptionRecruiterHandler.Handle(command, D.JobDescription.Empty(), _service, _clock,
                CancellationToken.None);

        // Assert
        var exception = await Assert
            .ThrowsAsync<NotFoundException>(async () => await Act());

        Assert.Contains("User not found", exception.Message);
    }
}