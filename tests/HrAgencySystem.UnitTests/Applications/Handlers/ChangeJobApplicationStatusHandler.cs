using HrAgencySystem.Recruitment.Application.JobApplications.ChangeStatus;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Applications.Handlers;

public class ChangeJobApplicationStatusHandlerTests
{
    private readonly IUserSnapshotRepository _snapshotRepository =
        Substitute.For<IUserSnapshotRepository>();

    private readonly INoteRepository _noteRepository =
        Substitute.For<INoteRepository>();

    private readonly IClock _clock =
        Substitute.For<IClock>();

    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _candidateId = Guid.NewGuid();
    private readonly Guid _jobApplicationId = Guid.NewGuid();
    private readonly Guid _modifiedBy = Guid.NewGuid();

    private readonly DateTimeOffset _now =
        new(2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

    private readonly UserSnapshot _user;

    public ChangeJobApplicationStatusHandlerTests()
    {
        _user = new UserSnapshot(
            _modifiedBy,
            "John",
            "Smith",
            "john.smith@example.com");

        _clock.UtcNow.Returns(_now);

        _snapshotRepository
            .GetUserAsync(_modifiedBy, Arg.Any<CancellationToken>())
            .Returns(_user);
    }

    [Fact]
    public async Task Should_change_status_to_screening()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Applied);

        var command = CreateCommand(JobApplicationUpdateStatus.Screening);

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationStatus.Applied, result.OldStatus);
        Assert.Equal(JobApplicationUpdateStatus.Screening, result.NewStatus);

        Assert.Equal(2, events.Count);

        var screeningEvent = Assert.IsType<JobApplicationScreeningStarted>(events[0]);
        Assert.Equal(_jobApplicationId, screeningEvent.JobApplicationId);
        Assert.Equal(_now, screeningEvent.OccurredAt);
        Assert.Equal(_user, screeningEvent.Author);

        var statusChangedEvent = Assert.IsType<JobApplicationStatusChanged>(events[1]);
        Assert.Equal(_jobApplicationId, statusChangedEvent.JobApplicationId);
        Assert.Equal(_candidateId, statusChangedEvent.CandidateId);
        Assert.Equal(JobApplicationStatus.Applied, statusChangedEvent.OldStatus);
        Assert.Equal(JobApplicationStatus.Screening, statusChangedEvent.NewStatus);
        Assert.Equal(_now, statusChangedEvent.OccurredAt);
        Assert.Equal(_user, statusChangedEvent.Author);
    }

    [Fact]
    public async Task Should_change_status_to_interview()
    {
        var interviewId = Guid.NewGuid();

        var aggregate = CreateAggregate(JobApplicationStatus.Screening);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Interview,
            interviewId: interviewId);

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationStatus.Screening, result.OldStatus);
        Assert.Equal(JobApplicationUpdateStatus.Interview, result.NewStatus);

        Assert.Equal(2, events.Count);

        var interviewEvent =
            Assert.IsType<JobApplicationInterviewScheduled>(events[0]);

        Assert.Equal(_jobApplicationId, interviewEvent.JobApplicationId);
        Assert.Equal(interviewId, interviewEvent.InterviewId);
        Assert.Equal(_now, interviewEvent.OccurredAt);
        Assert.Equal(_user, interviewEvent.Author);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(JobApplicationStatus.Interview, statusChangedEvent.NewStatus);
    }

    [Fact]
    public async Task Should_change_status_to_assessment()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Interview);

        var command = CreateCommand(JobApplicationUpdateStatus.Assessment);

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationStatus.Interview, result.OldStatus);
        Assert.Equal(JobApplicationUpdateStatus.Assessment, result.NewStatus);

        Assert.Equal(2, events.Count);

        Assert.IsType<JobApplicationAssessmentStarted>(events[0]);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(JobApplicationStatus.Assessment, statusChangedEvent.NewStatus);
    }

    [Fact]
    public async Task Should_change_status_to_offer()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Assessment);

        var command = CreateCommand(JobApplicationUpdateStatus.Offer);

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationStatus.Assessment, result.OldStatus);
        Assert.Equal(JobApplicationUpdateStatus.Offer, result.NewStatus);

        Assert.Equal(2, events.Count);

        Assert.IsType<JobApplicationOfferMade>(events[0]);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(JobApplicationStatus.Offer, statusChangedEvent.NewStatus);
    }

    [Fact]
    public async Task Should_change_status_to_hired()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Offer);

        var command = CreateCommand(JobApplicationUpdateStatus.Hired);

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationStatus.Offer, result.OldStatus);
        Assert.Equal(JobApplicationUpdateStatus.Hired, result.NewStatus);

        Assert.Equal(2, events.Count);

        Assert.IsType<JobApplicationHired>(events[0]);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(JobApplicationStatus.Hired, statusChangedEvent.NewStatus);
    }

    [Fact]
    public async Task Should_change_status_to_rejected()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Screening);

        var command = CreateCommand(JobApplicationUpdateStatus.Rejected);

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationStatus.Screening, result.OldStatus);
        Assert.Equal(JobApplicationUpdateStatus.Rejected, result.NewStatus);

        Assert.Equal(2, events.Count);

        Assert.IsType<JobApplicationRejected>(events[0]);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(JobApplicationStatus.Rejected, statusChangedEvent.NewStatus);
    }

    [Fact]
    public async Task Should_change_status_to_withdrawn()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Screening);

        var command = CreateCommand(JobApplicationUpdateStatus.Withdrawn);

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationStatus.Screening, result.OldStatus);
        Assert.Equal(JobApplicationUpdateStatus.Withdrawn, result.NewStatus);

        Assert.Equal(2, events.Count);

        Assert.IsType<JobApplicationWithdrawn>(events[0]);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(JobApplicationStatus.Withdrawn, statusChangedEvent.NewStatus);
    }

    [Fact]
    public async Task Should_add_note_when_note_is_provided()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Screening);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Interview,
            interviewId: Guid.NewGuid(),
            note: "Candidate passed the screening.");

        var (result, events) = await Handle(command, aggregate);

        Assert.Equal(JobApplicationUpdateStatus.Interview, result.NewStatus);
        Assert.Equal(3, events.Count);

        Assert.IsType<JobApplicationInterviewScheduled>(events[0]);
        Assert.IsType<JobApplicationStatusChanged>(events[1]);

        var noteEvent = Assert.IsType<JobApplicationNoteAdded>(events[2]);

        Assert.Equal(_jobApplicationId, noteEvent.JobApplicationId);
        Assert.Equal(_candidateId, noteEvent.CandidateId);
        Assert.Equal(_now, noteEvent.OccurredAt);
        Assert.Equal("Candidate passed the screening.", noteEvent.Note);
        Assert.Equal(_user, noteEvent.Author);

        await _noteRepository.Received(1).CreateNoteAsync(
            Arg.Is<CreateNoteDocument>(x =>
                x.JobApplicationId == _jobApplicationId &&
                x.OrganizationId == _organizationId &&
                x.CandidateId == _candidateId &&
                x.Text.Value == "Candidate passed the screening."),
            _user);
    }

    [Fact]
    public async Task Should_not_create_note_when_note_is_empty()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Screening);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Assessment,
            note: string.Empty);

        var (_, events) = await Handle(command, aggregate);

        Assert.Equal(2, events.Count);
        Assert.IsType<JobApplicationAssessmentStarted>(events[0]);
        Assert.IsType<JobApplicationStatusChanged>(events[1]);

        await _noteRepository
            .DidNotReceive()
            .CreateNoteAsync(
                Arg.Any<CreateNoteDocument>(),
                Arg.Any<UserSnapshot>());
    }

    [Fact]
    public async Task Should_not_create_note_when_note_is_null()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Screening);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Assessment,
            note: null!);

        var (_, events) = await Handle(command, aggregate);

        Assert.Equal(2, events.Count);

        await _noteRepository
            .DidNotReceive()
            .CreateNoteAsync(
                Arg.Any<CreateNoteDocument>(),
                Arg.Any<UserSnapshot>());
    }

    [Fact]
    public async Task Should_throw_when_organization_does_not_match()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Applied);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Screening,
            organizationId: Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(command, aggregate));

        Assert.Equal(
            IOrganizationChecker.OrganizationCheckMessage,
            exception.Message);

        await _snapshotRepository
            .DidNotReceive()
            .GetUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_throw_when_modified_by_user_does_not_exist()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Applied);

        var missingUserId = Guid.NewGuid();

        _snapshotRepository
            .GetUserAsync(missingUserId, Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Screening,
            modifiedBy: missingUserId);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(command, aggregate));

        Assert.Equal(
            IUserSnapshotRepository.NotFoundMessage,
            exception.Message);
    }

    [Fact]
    public async Task Should_throw_when_interview_id_is_missing()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Screening);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Interview,
            interviewId: null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(command, aggregate));

        Assert.Equal(
            "Interview id must be specified.",
            exception.Message);

        await _noteRepository
            .DidNotReceive()
            .CreateNoteAsync(
                Arg.Any<CreateNoteDocument>(),
                Arg.Any<UserSnapshot>());
    }

    [Fact]
    public async Task Should_throw_when_status_change_is_not_allowed()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Applied);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Assessment);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(command, aggregate));

        Assert.Contains(
            "Not allowed to change job application status",
            exception.Message);
    }
    

    [Fact]
    public async Task Should_use_clock_time_for_all_events()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Applied);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Screening);

        var (_, events) = await Handle(command, aggregate);

        var screeningEvent =
            Assert.IsType<JobApplicationScreeningStarted>(events[0]);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(_now, screeningEvent.OccurredAt);
        Assert.Equal(_now, statusChangedEvent.OccurredAt);
    }

    [Fact]
    public async Task Should_use_modified_by_snapshot_as_event_author()
    {
        var aggregate = CreateAggregate(JobApplicationStatus.Applied);

        var command = CreateCommand(
            JobApplicationUpdateStatus.Screening);

        var (_, events) = await Handle(command, aggregate);

        var screeningEvent =
            Assert.IsType<JobApplicationScreeningStarted>(events[0]);

        var statusChangedEvent =
            Assert.IsType<JobApplicationStatusChanged>(events[1]);

        Assert.Equal(_user, screeningEvent.Author);
        Assert.Equal(_user, statusChangedEvent.Author);
    }

    private async Task<(ChangeJobApplicationStatusResult Result, Wolverine.Marten.Events Events)>
        Handle(
            ChangeJobApplicationStatus command,
            JobApplication aggregate)
    {
        return await ChangeJobApplicationStatusHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            _noteRepository,
            _clock,
            CancellationToken.None);
    }

    private ChangeJobApplicationStatus CreateCommand(
        JobApplicationUpdateStatus status,
        Guid? interviewId = null,
        string? note = "",
        Guid? organizationId = null,
        Guid? modifiedBy = null)
    {
        return new ChangeJobApplicationStatus(
            _jobApplicationId,
            organizationId ?? _organizationId,
            note!,
            status,
            interviewId,
            modifiedBy ?? _modifiedBy);
    }

    private JobApplication CreateAggregate(JobApplicationStatus status)
    {
        var application = JobApplication.Empty();
        var now = DateTimeOffset.UtcNow;
        
        application.Apply(GetCreatedEvent(now));

        ChangeStatusPipeline(status, application, now);
        
        ApplyStatusEvent(
            application,
            status,
            _jobApplicationId,
            now);

        return application;
    }

    private void ChangeStatusPipeline(JobApplicationStatus status, JobApplication application, DateTimeOffset now)
    {
        switch (status)
        {
            case JobApplicationStatus.Assessment:
                ApplyStatusEvent(
                    application,
                    JobApplicationStatus.Screening,
                    _jobApplicationId,
                    now);
                break;
            case JobApplicationStatus.Interview or JobApplicationStatus.Hired or JobApplicationStatus.Offer:
            {
                ApplyStatusEvent(
                    application,
                    JobApplicationStatus.Screening,
                    _jobApplicationId,
                    now);
            
                ApplyStatusEvent(
                    application,
                    JobApplicationStatus.Assessment,
                    _jobApplicationId,
                    now);
            
                if (status == JobApplicationStatus.Hired)
                    ApplyStatusEvent(
                        application,
                        JobApplicationStatus.Offer,
                        _jobApplicationId,
                        now);
                break;
            }
        }
    }

    private JobApplicationCreated GetCreatedEvent(DateTimeOffset now)
    {
        return new JobApplicationCreated(
            _jobApplicationId,
            _organizationId,
            _candidateId,
            "Senior Software Developer",
            CandidateSource.InternalDatabase,
            new CompanySnapshot(
                Guid.NewGuid(),
                "Acme Corporation",
                "acme.example.com"),
            new CandidateInfo(
                _candidateId,
                "john.smith@example.com",
                "",
                "John",
                "Smith"),
            "john.smith@example.com",
            "+48 600 123 456",
            "John",
            "Smith",
            now);
    }

    private void ApplyStatusEvent(
        JobApplication application,
        JobApplicationStatus status,
        Guid jobApplicationId,
        DateTimeOffset now)
    {
        switch (status)
        {
            case JobApplicationStatus.Applied:
                return;

            case JobApplicationStatus.Hired:
                application.Apply(
                    new JobApplicationHired(
                        jobApplicationId,
                        now,
                        _user));
                return;

            case JobApplicationStatus.Screening:
                application.Apply(
                    new JobApplicationScreeningStarted(
                        jobApplicationId,
                        now,
                        _user));
                return;

            case JobApplicationStatus.Interview:
                application.Apply(
                    new JobApplicationInterviewScheduled(
                        jobApplicationId,
                        now,
                        _user,
                        Guid.NewGuid()));
                return;

            case JobApplicationStatus.Assessment:
                application.Apply(
                    new JobApplicationAssessmentStarted(
                        jobApplicationId,
                        now,
                        _user));
                return;

            case JobApplicationStatus.Offer:
                application.Apply(
                    new JobApplicationOfferMade(
                        jobApplicationId,
                        now,
                        _user));
                return;

            case JobApplicationStatus.Rejected:
                application.Apply(
                    new JobApplicationRejected(
                        jobApplicationId,
                        now,
                        _user));
                return;

            case JobApplicationStatus.Withdrawn:
                application.Apply(
                    new JobApplicationWithdrawn(
                        jobApplicationId,
                        now,
                        _user));
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}