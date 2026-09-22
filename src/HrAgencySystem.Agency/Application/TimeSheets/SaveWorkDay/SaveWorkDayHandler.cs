using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Agency.Application.TimeSheets.SaveWorkDay;

/// <summary>
/// Records one day. Not an aggregate handler, because the first day of a month is also the first
/// event of that month's stream - the sheet is created by being written on, not by a separate
/// "open the month" step nobody would remember to press.
/// </summary>
public static class SaveWorkDayHandler
{
    public static async Task<WorkDaySaved> Handle(
        SaveWorkDay command,
        IAgencyService service,
        IAgencyEmploymentQueryRepository employments,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var organizationId = OrganizationId.From(command.OrganizationId);

        var (period, periodError) = TimeSheetPeriod.TryCreate(command.Year, command.Month);

        if (periodError is not null)
            throw new ValidationException(periodError);

        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        // The current month counts as started; the next one does not exist to be filled in yet.
        if (!period!.HasStartedBy(today))
            throw new BusinessRuleException(TimeSheetPeriod.FutureMessage);

        if (!period.Contains(command.Date))
            throw new BusinessRuleException(TimeSheetRules.DateOutsideMonthMessage);

        var (duration, durationError) = WorkDuration.TryCreate(command.Hours, command.Minutes);

        if (durationError is not null)
            throw new ValidationException(durationError);

        await EnsureCovered(employments, organizationId, command.UserId, period, ct);

        var (note, noteError) = ShortNote.TryCreate(command.Note ?? "");

        if (noteError is not null)
            throw new ValidationException(noteError);

        var user = await service.GetOrganizationMemberAsync(organizationId, command.UserId, ct);
        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var streamId = AgencyStreamId.ForTimeSheet(
            command.OrganizationId,
            command.UserId,
            period.Year,
            period.Month
        );

        var sheet = await session.Events.AggregateStreamAsync<TimeSheet>(streamId, token: ct);

        var day = new WorkDay(
            command.Date,
            command.StartsAt,
            duration!.TotalMinutes,
            note?.Value ?? ""
        );

        var saved = new WorkDaySaved(
            command.OrganizationId,
            command.UserId,
            period.Year,
            period.Month,
            day,
            modifiedBy,
            clock.UtcNow
        );

        if (sheet is null)
        {
            var started = new TimeSheetStarted(
                command.OrganizationId,
                command.UserId,
                user,
                period.Year,
                period.Month,
                modifiedBy,
                clock.UtcNow
            );

            session.Events.StartStream<TimeSheet>(streamId, started, saved);
        }
        else
        {
            service.ValidateAggregateUpdate(sheet, command.OrganizationId);
            TimeSheetRules.EnsureEditable(sheet);

            await session.Events.AppendExclusive(streamId, saved);
        }

        return saved;
    }

    /// <summary>
    /// Hours are recorded against an engagement, not against an account. Somebody invoicing us has
    /// nothing to record, and a month outside their engagement is not theirs to fill in.
    /// </summary>
    private static async Task EnsureCovered(
        IAgencyEmploymentQueryRepository employments,
        OrganizationId organizationId,
        Guid userId,
        TimeSheetPeriod period,
        CancellationToken ct
    )
    {
        var employment =
            await employments.GetAsync(organizationId, userId, ct)
            ?? throw new BusinessRuleException(TimeSheetRules.NoEmploymentMessage);

        if (!employment.RequiresTimeRecord)
            throw new BusinessRuleException(TimeSheetRules.NotCoveredMessage);

        if (!employment.CoversMonth(period.Year, period.Month))
            throw new BusinessRuleException(
                "That month falls outside this person's engagement with us."
            );
    }
}
