using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// Builds a sheet the only way a sheet can come about: by replaying the events that wrote it.
/// </summary>
internal static class TimeSheetScenario
{
    public const int Year = 2026;
    public const int Month = 9;

    public static UserSnapshot Owner { get; } =
        new(OrgScenario.PayrollSpecialist, "Ewa", "Nowicka", "ewa.nowicka@hr-agency.com");

    public static DateOnly Day(int day) => new(Year, Month, day);

    public static TimeSheet Empty(Guid? userId = null)
    {
        var sheet = TimeSheet.Empty();

        sheet.Apply(
            new TimeSheetStarted(
                OrgScenario.OrganizationId,
                userId ?? OrgScenario.PayrollSpecialist,
                Owner,
                Year,
                Month,
                Owner,
                DateTimeOffset.UtcNow
            )
        );

        return sheet;
    }

    /// <summary>A draft with one eight hour day on it - enough to be sent for approval.</summary>
    public static TimeSheet WithOneDay(Guid? userId = null)
    {
        var sheet = Empty(userId);

        sheet.Apply(DaySaved(Day(1), 8 * 60));

        return sheet;
    }

    public static TimeSheet Submitted(Guid? userId = null)
    {
        var sheet = WithOneDay(userId);

        sheet.Apply(
            new TimeSheetSubmitted(
                OrgScenario.OrganizationId,
                sheet.UserId,
                Year,
                Month,
                sheet.TotalMinutes,
                Owner,
                DateTimeOffset.UtcNow
            )
        );

        return sheet;
    }

    public static TimeSheet Approved(Guid? userId = null)
    {
        var sheet = Submitted(userId);

        sheet.Apply(
            new TimeSheetApproved(
                OrgScenario.OrganizationId,
                sheet.UserId,
                Year,
                Month,
                OrgScenario.User,
                DateTimeOffset.UtcNow
            )
        );

        return sheet;
    }

    public static WorkDaySaved DaySaved(DateOnly date, int minutes) =>
        new(
            OrgScenario.OrganizationId,
            OrgScenario.PayrollSpecialist,
            Year,
            Month,
            new WorkDay(date, new TimeOnly(8, 0), minutes, ""),
            Owner,
            DateTimeOffset.UtcNow
        );
}
