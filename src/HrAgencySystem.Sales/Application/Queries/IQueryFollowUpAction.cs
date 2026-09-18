using HrAgencySystem.Sales.Documents;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Sales.Application.Queries;

public interface IQueryFollowUpAction
{
    Task<SliceResponse<FollowUpAction>> GetSlicesAsync(Guid organizationId, FollowUpActionQuery query,
        CancellationToken ct);

    Task<FollowUpAction?> GetByIdAsync(Guid organizationId, Guid followUpActionId, CancellationToken ct);

    Task<FollowUpAction?> GetLatestAsync(Guid organizationId, Guid opportunityId, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record FollowUpActionQuery(
    Guid? OpportunityId,
    Guid? CompanyId,
    int Page,
    int PageSize) : IPagedQuery;
