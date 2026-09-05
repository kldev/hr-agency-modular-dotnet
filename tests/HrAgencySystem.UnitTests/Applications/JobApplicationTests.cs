using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Applications;

public sealed class JobApplicationTests
{
    private static readonly Guid TestApplicationId = Guid.NewGuid();
    private static readonly Guid TestOrganizationId = Guid.NewGuid();
    private static readonly Guid TestJobPostId = Guid.NewGuid();
    private static readonly Guid TestCandidateId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestInterviewId = Guid.NewGuid();
    private static readonly UserSnapshot TestUser = new(Guid.NewGuid(), "F", "L", "fl@test.pl");

    private const string EmailAddress = "candidate@example.com";

    [Fact]
    public void Empty_should_create_empty_application()
    {
        var application = JobApplication.Empty();

        Assert.Equal(default, application.Id);
        Assert.Equal(default, application.OrganizationId);
        Assert.Equal(default, application.JobPostId);
        Assert.Equal(default, application.CandidateId);
        Assert.Equal(default, application.Status);
        Assert.Equal(default, application.Source);
        Assert.Equal(default, application.CreatedAt);
        Assert.Equal(default, application.UpdatedAt);
        Assert.Null(application.Email);
        Assert.Null(application.LastModifiedByUserId);
        Assert.Null(application.LastModifiedByUser);
        Assert.Null(application.LatestInterviewId);
    }

    [Fact]
    public void Apply_created_should_initialize_application()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var @event = CreateApplicationCreated(createdAt);

        var application = JobApplication.Empty();

        application.Apply(@event);

        Assert.Equal(
            JobApplicationId.From(TestApplicationId),
            application.Id);

        Assert.Equal(
            OrganizationId.From(TestOrganizationId),
            application.OrganizationId);

        Assert.Equal(
            JobPostId.From(TestJobPostId),
            application.JobPostId);

        Assert.Equal(
            CandidateId.From(TestCandidateId),
            application.CandidateId);

        Assert.Equal(
            JobApplicationStatus.Applied,
            application.Status);

        Assert.Equal(
            CandidateSource.Direct,
            application.Source);

        Assert.Equal(createdAt, application.CreatedAt);
        Assert.Equal(createdAt, application.UpdatedAt);

        Assert.Equal(
            Email.Create(EmailAddress),
            application.Email);

        Assert.Null(application.LastModifiedByUserId);
        Assert.Null(application.LastModifiedByUser);
        Assert.Null(application.LatestInterviewId);
    }

    [Fact]
    public void Apply_screening_started_should_change_applied_to_screening()
    {
        var application = GivenApplication();

        var occurredAt = DateTimeOffset.UtcNow;
        var author = CreateUserSnapshot();

        application.Apply(
            CreateScreeningStarted(
                occurredAt,
                TestUserId,
                author));

        Assert.Equal(
            JobApplicationStatus.Screening,
            application.Status);

        Assert.Equal(
            occurredAt,
            application.UpdatedAt);

        Assert.Equal(
            TestUserId,
            application.LastModifiedByUserId);

        Assert.Same(
            author,
            application.LastModifiedByUser);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Screening)]
    [InlineData(JobApplicationStatus.Interview)]
    public void Apply_assessment_started_should_change_allowed_status_to_assessment(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var occurredAt = DateTimeOffset.UtcNow;
        var author = CreateUserSnapshot();

        application.Apply(
            CreateAssessmentStarted(
                occurredAt,
                TestUserId,
                author));

        Assert.Equal(
            JobApplicationStatus.Assessment,
            application.Status);

        Assert.Equal(
            occurredAt,
            application.UpdatedAt);

        Assert.Equal(
            TestUserId,
            application.LastModifiedByUserId);

        Assert.Same(
            author,
            application.LastModifiedByUser);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Applied)]
    [InlineData(JobApplicationStatus.Offer)]
    [InlineData(JobApplicationStatus.Hired)]
    [InlineData(JobApplicationStatus.Rejected)]
    [InlineData(JobApplicationStatus.Withdrawn)]
    public void Apply_assessment_started_should_reject_invalid_status(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var exception = Assert.Throws<InvalidOperationException>(
            () => application.Apply(CreateAssessmentStarted()));

        Assert.Equal(
            $"Cannot change application status from {currentStatus}.",
            exception.Message);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Screening)]
    [InlineData(JobApplicationStatus.Assessment)]
    public void Apply_interview_scheduled_should_change_allowed_status_to_interview(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var occurredAt = DateTimeOffset.UtcNow;
        var author = CreateUserSnapshot();

        application.Apply(
            CreateInterviewScheduled(
                TestInterviewId,
                occurredAt,
                TestUserId,
                author));

        Assert.Equal(
            JobApplicationStatus.Interview,
            application.Status);

        Assert.Equal(
            TestInterviewId,
            application.LatestInterviewId);

        Assert.Equal(
            occurredAt,
            application.UpdatedAt);

        Assert.Equal(
            TestUserId,
            application.LastModifiedByUserId);

        Assert.Same(
            author,
            application.LastModifiedByUser);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Applied)]
    [InlineData(JobApplicationStatus.Offer)]
    [InlineData(JobApplicationStatus.Hired)]
    [InlineData(JobApplicationStatus.Rejected)]
    [InlineData(JobApplicationStatus.Withdrawn)]
    public void Apply_interview_scheduled_should_reject_invalid_status(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var exception = Assert.Throws<InvalidOperationException>(
            () => application.Apply(CreateInterviewScheduled()));

        Assert.Equal(
            $"Cannot change application status from {currentStatus}.",
            exception.Message);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Interview)]
    [InlineData(JobApplicationStatus.Assessment)]
    public void Apply_offer_made_should_change_allowed_status_to_offer(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var occurredAt = DateTimeOffset.UtcNow;
        var author = CreateUserSnapshot();

        application.Apply(
            CreateOfferMade(
                occurredAt,
                TestUserId,
                author));

        Assert.Equal(
            JobApplicationStatus.Offer,
            application.Status);

        Assert.Equal(
            occurredAt,
            application.UpdatedAt);

        Assert.Equal(
            TestUserId,
            application.LastModifiedByUserId);

        Assert.Same(
            author,
            application.LastModifiedByUser);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Applied)]
    [InlineData(JobApplicationStatus.Screening)]
    [InlineData(JobApplicationStatus.Offer)]
    [InlineData(JobApplicationStatus.Hired)]
    [InlineData(JobApplicationStatus.Rejected)]
    [InlineData(JobApplicationStatus.Withdrawn)]
    public void Apply_offer_made_should_reject_invalid_status(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var exception = Assert.Throws<InvalidOperationException>(
            () => application.Apply(CreateOfferMade()));

        Assert.Equal(
            $"Cannot change application status from {currentStatus}.",
            exception.Message);
    }

    [Fact]
    public void Apply_hired_should_change_application_to_hired()
    {
        var application = GivenApplication(JobApplicationStatus.Offer);

        var occurredAt = DateTimeOffset.UtcNow;
        var author = CreateUserSnapshot();

        application.Apply(
            CreateHired(
                occurredAt,
                TestUserId,
                author));

        Assert.Equal(
            JobApplicationStatus.Hired,
            application.Status);

        Assert.Equal(
            occurredAt,
            application.UpdatedAt);

        Assert.Equal(
            TestUserId,
            application.LastModifiedByUserId);

        Assert.Same(
            author,
            application.LastModifiedByUser);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Applied)]
    [InlineData(JobApplicationStatus.Screening)]
    [InlineData(JobApplicationStatus.Assessment)]
    [InlineData(JobApplicationStatus.Interview)]
    [InlineData(JobApplicationStatus.Offer)]
    [InlineData(JobApplicationStatus.Rejected)]
    [InlineData(JobApplicationStatus.Withdrawn)]
    public void Apply_hired_should_change_any_status_to_hired(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        application.Apply(CreateHired());

        Assert.Equal(
            JobApplicationStatus.Hired,
            application.Status);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Applied)]
    [InlineData(JobApplicationStatus.Screening)]
    [InlineData(JobApplicationStatus.Assessment)]
    [InlineData(JobApplicationStatus.Interview)]
    [InlineData(JobApplicationStatus.Offer)]
    public void Apply_rejected_should_change_non_final_status_to_rejected(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var occurredAt = DateTimeOffset.UtcNow;
        var author = CreateUserSnapshot();

        application.Apply(
            CreateRejected(
                occurredAt,
                TestUserId,
                author));

        Assert.Equal(
            JobApplicationStatus.Rejected,
            application.Status);

        Assert.Equal(
            occurredAt,
            application.UpdatedAt);

        Assert.Equal(
            TestUserId,
            application.LastModifiedByUserId);

        Assert.Same(
            author,
            application.LastModifiedByUser);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Hired)]
    [InlineData(JobApplicationStatus.Rejected)]
    [InlineData(JobApplicationStatus.Withdrawn)]
    public void Apply_rejected_should_reject_final_status(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var exception = Assert.Throws<InvalidOperationException>(
            () => application.Apply(CreateRejected()));

        Assert.Equal(
            $"Application is already in final status: {currentStatus}.",
            exception.Message);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Applied)]
    [InlineData(JobApplicationStatus.Screening)]
    [InlineData(JobApplicationStatus.Assessment)]
    [InlineData(JobApplicationStatus.Interview)]
    [InlineData(JobApplicationStatus.Offer)]
    public void Apply_withdrawn_should_change_non_final_status_to_withdrawn(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var occurredAt = DateTimeOffset.UtcNow;
        var author = CreateUserSnapshot();

        application.Apply(
            CreateWithdrawn(
                occurredAt,
                TestUserId,
                author));

        Assert.Equal(
            JobApplicationStatus.Withdrawn,
            application.Status);

        Assert.Equal(
            occurredAt,
            application.UpdatedAt);

        Assert.Equal(
            TestUserId,
            application.LastModifiedByUserId);

        Assert.Same(
            author,
            application.LastModifiedByUser);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Hired)]
    [InlineData(JobApplicationStatus.Rejected)]
    [InlineData(JobApplicationStatus.Withdrawn)]
    public void Apply_withdrawn_should_reject_final_status(
        JobApplicationStatus currentStatus)
    {
        var application = GivenApplication(currentStatus);

        var exception = Assert.Throws<InvalidOperationException>(
            () => application.Apply(CreateWithdrawn()));

        Assert.Equal(
            $"Application is already in final status: {currentStatus}.",
            exception.Message);
    }

    private static JobApplication GivenApplication(
        JobApplicationStatus status = JobApplicationStatus.Applied)
    {
        var application = JobApplication.Empty();

        application.Apply(CreateApplicationCreated());

        if (status == JobApplicationStatus.Applied)
            return application;

        switch (status)
        {
            case JobApplicationStatus.Screening:
                application.Apply(CreateScreeningStarted());
                break;

            case JobApplicationStatus.Assessment:
                application.Apply(CreateScreeningStarted());
                application.Apply(CreateAssessmentStarted());
                break;

            case JobApplicationStatus.Interview:
                application.Apply(CreateScreeningStarted());
                application.Apply(CreateInterviewScheduled());
                break;

            case JobApplicationStatus.Offer:
                application.Apply(CreateScreeningStarted());
                application.Apply(CreateInterviewScheduled());
                application.Apply(CreateOfferMade());
                break;

            case JobApplicationStatus.Hired:
                application.Apply(CreateHired());
                break;

            case JobApplicationStatus.Rejected:
                application.Apply(CreateRejected());
                break;

            case JobApplicationStatus.Withdrawn:
                application.Apply(CreateWithdrawn());
                break;

            case JobApplicationStatus.Applied:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    null);
        }

        return application;
    }

    private static JobApplicationCreated CreateApplicationCreated(
        DateTimeOffset? createdAt = null)
    {
        return new JobApplicationCreated(
            TestApplicationId, 
            TestOrganizationId, 
            TestJobPostId, "Job Title",
            CandidateSource.Direct,
            new CompanySnapshot(Guid.NewGuid(), "", ""),
            new CandidateInfo(TestCandidateId, EmailAddress, "+1 112 123 124", "Test", "Last"),
            EmailAddress,
            "+1 112 123 124",
            "Test", "Last",
            createdAt ?? DateTimeOffset.UtcNow);
    }

    private static JobApplicationScreeningStarted CreateScreeningStarted(
        DateTimeOffset? occurredAt = null,
        Guid? authorId = null,
        UserSnapshot? author = null)
    {
        return new JobApplicationScreeningStarted(TestApplicationId, occurredAt ?? DateTimeOffset.UtcNow,
            authorId ?? Guid.NewGuid(),
            author ?? TestUser);
    }

    private static JobApplicationAssessmentStarted CreateAssessmentStarted(
        DateTimeOffset? occurredAt = null,
        Guid? authorId = null,
        UserSnapshot? author = null)
    {
        return new JobApplicationAssessmentStarted(TestApplicationId, occurredAt ?? DateTimeOffset.UtcNow,
            authorId ?? Guid.NewGuid(),
            author ?? TestUser);
    }

    private static JobApplicationInterviewScheduled CreateInterviewScheduled(
        Guid? interviewId = null,
        DateTimeOffset? occurredAt = null,
        Guid? authorId = null,
        UserSnapshot? author = null)
    {
        return new JobApplicationInterviewScheduled(TestApplicationId, occurredAt ?? DateTimeOffset.UtcNow,
            authorId ?? Guid.NewGuid(),
            author ?? TestUser, interviewId ?? Guid.NewGuid());
    }

    private static JobApplicationOfferMade CreateOfferMade(
        DateTimeOffset? occurredAt = null,
        Guid? authorId = null,
        UserSnapshot? author = null)
    {
        return new JobApplicationOfferMade(TestApplicationId, occurredAt ?? DateTimeOffset.UtcNow,
            authorId ?? Guid.NewGuid(),
            author ?? TestUser);
    }

    private static JobApplicationHired CreateHired(
        DateTimeOffset? occurredAt = null,
        Guid? authorId = null,
        UserSnapshot? author = null)
    {
        return new JobApplicationHired(TestApplicationId, occurredAt ?? DateTimeOffset.UtcNow,
            authorId ?? Guid.NewGuid(),
            author ?? TestUser);
    }

    private static JobApplicationRejected CreateRejected(
        DateTimeOffset? occurredAt = null,
        Guid? authorId = null,
        UserSnapshot? author = null)
    {
        return new JobApplicationRejected(TestApplicationId, occurredAt ?? DateTimeOffset.UtcNow,
            authorId ?? Guid.NewGuid(),
            author ?? TestUser);
    }

    private static JobApplicationWithdrawn CreateWithdrawn(
        DateTimeOffset? occurredAt = null,
        Guid? authorId = null,
        UserSnapshot? author = null)
    {
        return new JobApplicationWithdrawn(TestApplicationId, occurredAt ?? DateTimeOffset.UtcNow,
            authorId ?? Guid.NewGuid(),
            author ?? TestUser);
    }

    private static UserSnapshot CreateUserSnapshot()
    {
        return TestUser;
    }
}