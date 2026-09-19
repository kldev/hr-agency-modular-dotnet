using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.Recruitment.Application.JobPosting.ChangeRecruiter;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using NSubstitute;
using Wolverine;

namespace HrAgencySystem.UnitTests.JobPostings.Handlers;

public sealed class ChangeJobPostRecruiterHandlerTests
{
    private readonly IRecruitmentService _service = Substitute.For<IRecruitmentService>();

    private readonly IJobPostQueryRepository _queryRepository =
        Substitute.For<IJobPostQueryRepository>();

    private readonly IClock _clock = Substitute.For<IClock>();

    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid JobPostId = Guid.NewGuid();

    private static readonly UserSnapshot NewRecruiter = new(
        Guid.NewGuid(),
        "Katy",
        "Wells",
        "katy.wells@hr-agency.com"
    );

    private static readonly UserSnapshot ChangedBy = new(
        Guid.NewGuid(),
        "John",
        "Smith",
        "john.smith@hr-agency.com"
    );

    [Fact]
    public async Task Handle_WithRecruiterOtherThanModifier_SendsNotification()
    {
        var (_, _, messages) = await Handle(NewRecruiter, ChangedBy);

        var notification = Assert.Single(messages.OfType<SendJobPostRecruiterChanged>());
        Assert.Equal(JobPostId, notification.JobPostId);
        Assert.Equal("Senior .NET Developer", notification.JobPostTitle);
        Assert.Equal(NewRecruiter.Email, notification.RecruiterEmail);
        Assert.Equal(NewRecruiter.Fullname, notification.RecruiterFullname);
        Assert.Equal(ChangedBy.Fullname, notification.ChangedByFullname);
    }

    [Fact]
    public async Task Handle_WithModifierTakingItOver_SendsNoNotification()
    {
        var (_, _, messages) = await Handle(ChangedBy, ChangedBy);

        Assert.Empty(messages);
        await _queryRepository
            .DidNotReceive()
            .GetJobPostInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private async Task<(JobPostRecruiterChanged, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        UserSnapshot recruiter,
        UserSnapshot modifiedBy
    )
    {
        var command = new ChangeJobPostRecruiter(
            JobPostId,
            OrganizationId,
            recruiter.Id,
            modifiedBy.Id
        );

        _service.GetUserAsync(recruiter.Id, Arg.Any<CancellationToken>()).Returns(recruiter);
        _service.GetUserAsync(modifiedBy.Id, Arg.Any<CancellationToken>()).Returns(modifiedBy);
        _queryRepository
            .GetJobPostInfo(JobPostId, Arg.Any<CancellationToken>())
            .Returns(
                new JobPostInfo(
                    JobPostId,
                    OrganizationId,
                    Guid.NewGuid(),
                    "Senior .NET Developer",
                    JobPostStatus.Published,
                    recruiter
                )
            );
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 9, 19, 10, 0, 0, TimeSpan.Zero));

        return await ChangeJobPostRecruiterHandler.Handle(
            command,
            JobPost.WithOrganization(OrganizationId),
            _service,
            _queryRepository,
            _clock,
            CancellationToken.None
        );
    }
}
