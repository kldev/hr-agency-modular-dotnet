using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Application.Port;

/// <summary>
/// The read side of the register: three questions, because there are three screens.
/// </summary>
public interface ITimeSheetQueryRepository
{
    Task<TimeSheetProjection?> GetAsync(
        OrganizationId organizationId,
        Guid userId,
        int year,
        int month,
        CancellationToken ct
    );

    /// <summary>
    /// The monitoring screen: everybody below this person in the chart who is covered by the duty
    /// to record hours, whether or not they have started a sheet.
    /// <para>
    /// The rows without a sheet are the entire point. This is the screen somebody opens every day -
    /// approval happens once a month, chasing people happens all the time - and a list that only
    /// showed the sheets that exist would hide exactly the people worth chasing.
    /// </para>
    /// </summary>
    Task<IReadOnlyList<TeamTimeSheetRow>> GetTeamAsync(
        OrganizationId organizationId,
        Guid supervisorUserId,
        int year,
        int month,
        CancellationToken ct
    );

    /// <summary>For payroll: everything agreed for the month, settled or waiting to be.</summary>
    Task<IReadOnlyList<TimeSheetProjection>> GetForSettlementAsync(
        OrganizationId organizationId,
        int year,
        int month,
        CancellationToken ct
    );
}

/// <summary>
/// One person on the monitoring list. <paramref name="Status"/> is null for somebody who has not
/// started - which is a state worth its own answer rather than a zero.
/// </summary>
public sealed record TeamTimeSheetRow(
    Guid UserId,
    UserSnapshot User,
    TimeSheetStatus? Status,
    int TotalMinutes,
    int FilledDays,
    DateOnly? LastEntryOn,
    DateTimeOffset? SubmittedAt
)
{
    public bool HasStarted => Status is not null;
}
