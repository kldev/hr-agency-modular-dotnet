using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Projections.Reports;
using HrAgencySystem.Reports.ReadModel;
using HrAgencySystem.SharedKernel.Snapshots;
using JasperFx.Events;

namespace HrAgencySystem.UnitTests.Reports;

public sealed class ApplicationReportProjectionTests
{
    private static readonly Guid ApplicationId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid JobPostId = Guid.NewGuid();
    private static readonly UserSnapshot Author = new(Guid.NewGuid(), "F", "L", "fl@test.pl");
    private static readonly DateTimeOffset Day1 = new(2026, 3, 2, 9, 0, 0, TimeSpan.Zero);

    private readonly ApplicationReportProjection _projection = new();

    [Fact]
    public void Created_StartsTheRowAsApplied()
    {
        var row = Project(Created());

        Assert.NotNull(row);
        Assert.Equal(ApplicationId, row.Id);
        Assert.Equal(OrganizationId, row.OrganizationId);
        Assert.Equal(JobPostId, row.JobPostId);
        Assert.Equal(nameof(CandidateSource.JustJoinIt), row.Source);
        Assert.Equal(nameof(JobApplicationStatus.Applied), row.Status);
        Assert.Equal(Day1, row.CreatedAt);
        Assert.Equal(Day1, row.UpdatedAt);
        Assert.Null(row.ScreeningAt);
    }

    [Fact]
    public void InterviewScheduled_ReachesInterviewWithoutAStatusChangeEvent()
    {
        var row = Project(
            Created(),
            new JobApplicationInterviewScheduled(
                ApplicationId,
                Day1.AddDays(3),
                Author,
                Guid.NewGuid()
            )
        );

        Assert.Equal(nameof(JobApplicationStatus.Interview), row!.Status);
        Assert.Equal(Day1.AddDays(3), row.InterviewAt);
        Assert.Equal(Day1.AddDays(3), row.UpdatedAt);
    }

    [Fact]
    public void NarrativeEventAndStatusChange_CountTheStageOnce()
    {
        var row = Project(
            Created(),
            new JobApplicationOfferMade(ApplicationId, Day1.AddDays(5), Author),
            StatusChanged(
                JobApplicationStatus.Interview,
                JobApplicationStatus.Offer,
                Day1.AddDays(5)
            )
        );

        Assert.Equal(nameof(JobApplicationStatus.Offer), row!.Status);
        Assert.Equal(Day1.AddDays(5), row.OfferAt);
    }

    [Fact]
    public void ReachingAStageAgain_KeepsTheFirstTime()
    {
        var row = Project(
            Created(),
            new JobApplicationScreeningStarted(ApplicationId, Day1.AddDays(1), Author),
            new JobApplicationWithdrawn(ApplicationId, Day1.AddDays(2), Author),
            new JobApplicationReactivated(ApplicationId, Day1.AddDays(9), Author)
        );

        Assert.Equal(nameof(JobApplicationStatus.Screening), row!.Status);
        Assert.Equal(Day1.AddDays(1), row.ScreeningAt);
        Assert.Equal(Day1.AddDays(2), row.WithdrawnAt);
        Assert.Equal(Day1.AddDays(9), row.UpdatedAt);
    }

    [Fact]
    public void StatusChangeFromThePanel_ReachesTheStageItNames()
    {
        var row = Project(
            Created(),
            StatusChanged(
                JobApplicationStatus.Applied,
                JobApplicationStatus.Assessment,
                Day1.AddDays(4)
            )
        );

        Assert.Equal(nameof(JobApplicationStatus.Assessment), row!.Status);
        Assert.Equal(Day1.AddDays(4), row.AssessmentAt);
        Assert.Null(row.InterviewAt);
    }

    [Fact]
    public void Hired_SetsTheHiringDate()
    {
        var row = Project(
            Created(),
            new JobApplicationHired(ApplicationId, Day1.AddDays(20), Author)
        );

        Assert.Equal(nameof(JobApplicationStatus.Hired), row!.Status);
        Assert.Equal(Day1.AddDays(20), row.HiredAt);
    }

    [Fact]
    public void AnyOtherEventOfTheStream_OnlyMovesTheActivityDate()
    {
        var row = Project(
            Created(),
            new JobApplicationNoteAdded(
                ApplicationId,
                Guid.NewGuid(),
                Day1.AddDays(6),
                "note",
                Author
            )
        );

        Assert.Equal(nameof(JobApplicationStatus.Applied), row!.Status);
        Assert.Equal(Day1.AddDays(6), row.UpdatedAt);
    }

    [Fact]
    public void EventOfAStreamWithoutARow_IsIgnored()
    {
        var row = Project(new JobPostPublished(Guid.NewGuid(), Day1, Author));

        Assert.Null(row);
    }

    private ApplicationReportRow? Project(params object[] events)
    {
        ApplicationReportRow? row = null;

        foreach (var data in events)
        {
            var @event = Wrap(data);
            row = _projection.ApplyEvent(row, ApplicationId, @event, null!, null!);
        }

        return row;
    }

    /// <summary>
    /// Marten stamps an event with the time it was appended; here that is the event's own time, so
    /// the fallback for events the projection does not know is the same moment.
    /// </summary>
    private static IEvent Wrap(object data)
    {
        var at = data switch
        {
            JobApplicationCreated e => e.CreatedAt,
            JobApplicationNoteAdded e => e.OccurredAt,
            _ => Day1,
        };

        var type = typeof(Event<>).MakeGenericType(data.GetType());
        var @event = (IEvent)Activator.CreateInstance(type, data)!;
        @event.Timestamp = at;
        return @event;
    }

    private static JobApplicationCreated Created() =>
        new(
            ApplicationId,
            OrganizationId,
            JobPostId,
            "C# Developer",
            CandidateSource.JustJoinIt,
            new CompanySnapshot(Guid.NewGuid(), "ACME", "5252344078"),
            new CandidateInfo(Guid.NewGuid(), "a@b.pl", "+48 600 000 000", "Anna", "Nowak"),
            "a@b.pl",
            "+48 600 000 000",
            "Anna",
            "Nowak",
            Day1
        );

    private static JobApplicationStatusChanged StatusChanged(
        JobApplicationStatus from,
        JobApplicationStatus to,
        DateTimeOffset at
    ) => new(ApplicationId, Guid.NewGuid(), at, from, to, Author);
}
