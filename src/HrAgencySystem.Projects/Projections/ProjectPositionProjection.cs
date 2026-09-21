using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Projections;

/// <summary>
/// One document per position, so the roles can be listed, searched and filtered across projects -
/// which <c>ProjectProjection</c> cannot answer, because there a position is an element of an array
/// and Marten filters those far worse than it filters a column.
/// <para>
/// Only the project's id is carried here, not its name: the list needs the name, but a project that
/// is renamed would leave it stale on every one of its positions. The query repository reads both
/// documents - they belong to the same module, so that is a join it is allowed to make.
/// </para>
/// </summary>
public sealed record ProjectPositionProjection(
    Guid Id,
    Guid OrganizationId,
    Guid ProjectId,
    string Name,
    string ContractName,
    string WorkDescription,
    IReadOnlyList<string> Duties,
    IReadOnlyList<string> RequiredQualifications,
    WorkerContractType ContractType,
    WorkRate? ProposedRate,
    PostalAddress? WorkplaceAddress,
    decimal? WeeklyHours,
    TimeOnly? WorkStartsAt,
    string WorkSchedule,
    int? PayoutDay,
    string ProbationPeriod,
    string NoticePeriod,
    IReadOnlyList<string> Allowances,
    int? PlannedHeadcount,
    EngagementType? DefaultEngagementType,
    bool IsArchived,
    /// <summary>
    /// The assignments currently held against this role, kept as ids rather than a number: the
    /// integration events that maintain it are delivered at least once, and a set survives a
    /// repeat where an increment would not.
    /// </summary>
    IReadOnlyList<Guid> AssignedAssignmentIds,
    UserSnapshot OpenedBy,
    DateTimeOffset OpenedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
)
{
    public int AssignedCount => AssignedAssignmentIds.Count;

    /// <summary>
    /// How many people the role still waits for. Null when nobody said how many it needs - which
    /// is a different answer from "none missing" and has to read differently on the list.
    /// </summary>
    public int? MissingHeadcount =>
        PlannedHeadcount is null ? null : Math.Max(0, PlannedHeadcount.Value - AssignedCount);

    public static ProjectPositionProjection Create(ProjectPositionOpened @event) =>
        From(@event.ProjectId, @event.OrganizationId, @event.Position, []);

    public ProjectPositionProjection Apply(ProjectPositionUpdated @event) =>
        From(ProjectId, OrganizationId, @event.Position, AssignedAssignmentIds);

    public ProjectPositionProjection Apply(ProjectPositionArchived @event) =>
        this with
        {
            IsArchived = true,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };

    public ProjectPositionProjection Apply(ProjectPositionRestored @event) =>
        this with
        {
            IsArchived = false,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };

    private static ProjectPositionProjection From(
        Guid projectId,
        Guid organizationId,
        ProjectPosition position,
        IReadOnlyList<Guid> assigned
    ) =>
        new(
            position.PositionId,
            organizationId,
            projectId,
            position.Name,
            position.ContractName,
            position.WorkDescription,
            position.Duties,
            position.RequiredQualifications,
            position.ContractType,
            position.ProposedRate,
            position.WorkplaceAddress,
            position.WeeklyHours,
            position.WorkStartsAt,
            position.WorkSchedule,
            position.PayoutDay,
            position.ProbationPeriod,
            position.NoticePeriod,
            position.Allowances,
            position.PlannedHeadcount,
            position.DefaultEngagementType,
            position.IsArchived,
            assigned,
            position.OpenedBy,
            position.OpenedAt,
            position.ModifiedBy,
            position.ModifiedAt
        );
}
