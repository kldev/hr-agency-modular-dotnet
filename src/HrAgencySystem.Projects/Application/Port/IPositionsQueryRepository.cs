using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Projects.Application.Port;

public interface IPositionsQueryRepository
{
    Task<SliceResponse<PositionListItem>> GetPositions(
        OrganizationId organizationId,
        PositionQuery query,
        CancellationToken ct
    );

    Task<PositionDetails?> GetPosition(
        OrganizationId organizationId,
        Guid positionId,
        CancellationToken ct
    );
}

/// <summary>
/// Archived roles are out unless asked for: with twenty-five positions on a large client, most of
/// the list is history and the point of archiving is a shorter list.
/// </summary>
public sealed record PositionQuery(
    string Search,
    Guid? ProjectId,
    bool IncludeArchived,
    WorkerContractType? ContractType,
    int Page,
    int PageSize
) : IPagedQuery;

/// <summary>
/// A row of the register: the role plus the project it belongs to. The project's name is read at
/// query time rather than copied onto the position, so renaming a project cannot leave twenty-five
/// rows saying something else.
/// </summary>
public sealed record PositionListItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string ClientCompanyName,
    string WorkCountry,
    string Name,
    string ContractName,
    WorkerContractType ContractType,
    WorkRate? ProposedRate,
    decimal? WeeklyHours,
    int? PlannedHeadcount,
    int AssignedCount,
    int? MissingHeadcount,
    EngagementType? DefaultEngagementType,
    bool IsArchived,
    DateTimeOffset OpenedAt
);

/// <summary>
/// Everything the role says, plus where it is delivered. <paramref name="WorkplaceAddress"/> is the
/// answer, not the question: the position's own address when it has one, the project's otherwise.
/// </summary>
public sealed record PositionDetails(
    ProjectPositionProjection Position,
    Guid ProjectId,
    string ProjectName,
    string ClientCompanyName,
    string WorkCountry,
    PostalAddress WorkplaceAddress
);
