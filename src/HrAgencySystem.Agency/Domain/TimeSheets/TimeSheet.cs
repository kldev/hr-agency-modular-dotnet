using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Domain.TimeSheets;

/// <summary>
/// One person's hours for one month. The stream id is derived from the organization, the person and
/// the month, so "one sheet per person per month" is not a rule anybody has to enforce - there is
/// nowhere for a second one to go, and no lookup before a write.
/// </summary>
public sealed class TimeSheet : IOrganizationDomain
{
    /*
     * Both read through their properties: Marten replays an aggregate without running field
     * initialisers, so these are null on the first Apply of a rebuild.
     */
    private List<WorkDay>? _days = [];
    private List<TimeSheetComment>? _comments = [];

    private TimeSheet() { }

    /// <summary>An unwritten sheet, for replaying events onto - the same shape `OrgStructure` takes.</summary>
    public static TimeSheet Empty() => new();

    public OrganizationId OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }

    public TimeSheetStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ModifiedAt { get; private set; }
    public Guid? ModifiedById { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? SettledById { get; private set; }
    public DateTimeOffset? SettledAt { get; private set; }

    public IReadOnlyList<WorkDay> Days => _days ?? [];
    public IReadOnlyList<TimeSheetComment> Comments => _comments ?? [];

    public int TotalMinutes => Days.Sum(day => day.Minutes);

    public int FilledDays => Days.Count;

    public bool IsEditable => TimeSheetStatusChangePolicy.IsEditable(Status);

    public WorkDay? DayOn(DateOnly date) => Days.FirstOrDefault(day => day.Date == date);

    public void Apply(TimeSheetStarted @event)
    {
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        UserId = @event.UserId;
        Year = @event.Year;
        Month = @event.Month;
        Status = TimeSheetStatus.Draft;
        CreatedAt = @event.StartedAt;

        _days = [];
        _comments = [];

        Touch(@event.StartedBy, @event.StartedAt);
    }

    public void Apply(WorkDaySaved @event)
    {
        // An upsert by date, not another row: a day is a fact about that day, and saving it twice
        // is a correction rather than a second shift.
        _days = [.. Days.Where(day => day.Date != @event.Day.Date), @event.Day];

        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(WorkDayRemoved @event)
    {
        _days = [.. Days.Where(day => day.Date != @event.Date)];

        Touch(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(TimeSheetSubmitted @event)
    {
        Status = TimeSheetStatus.Submitted;
        SubmittedAt = @event.SubmittedAt;

        Touch(@event.SubmittedBy, @event.SubmittedAt);
    }

    public void Apply(TimeSheetApproved @event)
    {
        Status = TimeSheetStatus.Approved;
        ApprovedById = @event.ApprovedBy.Id;
        ApprovedAt = @event.ApprovedAt;

        Touch(@event.ApprovedBy, @event.ApprovedAt);
    }

    public void Apply(TimeSheetReturnedForCorrection @event)
    {
        Status = TimeSheetStatus.Correction;

        /*
         * The approval is cleared along with the status. A sheet that came back carrying who
         * approved it last time would show an answer to a question that is open again.
         */
        ApprovedById = null;
        ApprovedAt = null;

        // The reason travels inside the event, so a month cannot be sent back without one.
        _comments = [.. Comments, @event.Reason];

        Touch(@event.ReturnedBy, @event.ReturnedAt);
    }

    public void Apply(TimeSheetSettled @event)
    {
        Status = TimeSheetStatus.Settled;
        SettledById = @event.SettledBy.Id;
        SettledAt = @event.SettledAt;

        Touch(@event.SettledBy, @event.SettledAt);
    }

    public void Apply(TimeSheetCommented @event)
    {
        _comments = [.. Comments, @event.Comment];

        Touch(@event.Comment.Author, @event.Comment.At);
    }

    private void Touch(UserSnapshot by, DateTimeOffset at)
    {
        ModifiedAt = at;
        ModifiedById = by.Id;
    }
}
