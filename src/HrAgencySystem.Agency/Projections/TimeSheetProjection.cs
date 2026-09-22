using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Agency.Projections;

/// <summary>
/// One document per sheet: a person, a month and the days on it.
/// <para>
/// The number of working days in the month is not here, and that is on purpose - it is a fact about
/// the calendar, not about this sheet, and storing it would mean every sheet carrying a copy of an
/// answer anybody can work out.
/// </para>
/// </summary>
public sealed record TimeSheetProjection(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    UserSnapshot User,
    int Year,
    int Month,
    TimeSheetStatus Status,
    IReadOnlyList<WorkDay> Days,
    IReadOnlyList<TimeSheetComment> Comments,
    DateTimeOffset CreatedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt,
    DateTimeOffset? SubmittedAt,
    UserSnapshot? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    UserSnapshot? SettledBy,
    DateTimeOffset? SettledAt
)
{
    public int TotalMinutes => Days.Sum(day => day.Minutes);

    public int FilledDays => Days.Count;

    public DateOnly? LastEntryOn => Days.Count == 0 ? null : Days.Max(day => day.Date);

    public static TimeSheetProjection Create(TimeSheetStarted @event) =>
        new(
            AgencyStreamId.ForTimeSheet(
                @event.OrganizationId,
                @event.UserId,
                @event.Year,
                @event.Month
            ),
            @event.OrganizationId,
            @event.UserId,
            @event.User,
            @event.Year,
            @event.Month,
            TimeSheetStatus.Draft,
            [],
            [],
            @event.StartedAt,
            @event.StartedBy,
            @event.StartedAt,
            null,
            null,
            null,
            null,
            null
        );

    public TimeSheetProjection Apply(WorkDaySaved @event) =>
        (
            this with
            {
                Days = [.. Days.Where(day => day.Date != @event.Day.Date), @event.Day],
            }
        ).Touched(@event.ModifiedBy, @event.ModifiedAt);

    public TimeSheetProjection Apply(WorkDayRemoved @event) =>
        (this with { Days = [.. Days.Where(day => day.Date != @event.Date)] }).Touched(
            @event.ModifiedBy,
            @event.ModifiedAt
        );

    public TimeSheetProjection Apply(TimeSheetSubmitted @event) =>
        (
            this with
            {
                Status = TimeSheetStatus.Submitted,
                SubmittedAt = @event.SubmittedAt,
            }
        ).Touched(@event.SubmittedBy, @event.SubmittedAt);

    public TimeSheetProjection Apply(TimeSheetApproved @event) =>
        (
            this with
            {
                Status = TimeSheetStatus.Approved,
                ApprovedBy = @event.ApprovedBy,
                ApprovedAt = @event.ApprovedAt,
            }
        ).Touched(@event.ApprovedBy, @event.ApprovedAt);

    public TimeSheetProjection Apply(TimeSheetReturnedForCorrection @event) =>
        (
            this with
            {
                Status = TimeSheetStatus.Correction,
                ApprovedBy = null,
                ApprovedAt = null,
                Comments = [.. Comments, @event.Reason],
            }
        ).Touched(@event.ReturnedBy, @event.ReturnedAt);

    public TimeSheetProjection Apply(TimeSheetSettled @event) =>
        (
            this with
            {
                Status = TimeSheetStatus.Settled,
                SettledBy = @event.SettledBy,
                SettledAt = @event.SettledAt,
            }
        ).Touched(@event.SettledBy, @event.SettledAt);

    public TimeSheetProjection Apply(TimeSheetCommented @event) =>
        (this with { Comments = [.. Comments, @event.Comment] }).Touched(
            @event.Comment.Author,
            @event.Comment.At
        );

    private TimeSheetProjection Touched(UserSnapshot by, DateTimeOffset at) =>
        this with
        {
            ModifiedBy = by,
            ModifiedAt = at,
        };
}
