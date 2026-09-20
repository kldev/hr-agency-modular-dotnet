using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.Recruitment.Application.JobPosting.ChangeRecruiter;
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
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 9, 19, 10, 0, 0, TimeSpan.Zero));

        return await ChangeJobPostRecruiterHandler.Handle(
            command,
            JobPost.WithOrganization(OrganizationId, "Senior .NET Developer"),
            _service,
            _clock,
            CancellationToken.None
        );
    }
}
