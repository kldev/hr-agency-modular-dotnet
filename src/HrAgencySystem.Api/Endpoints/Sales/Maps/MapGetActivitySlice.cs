using HrAgencySystem.Api.Auth;
using HrAgencySystem.Sales.Application.Queries;

namespace HrAgencySystem.Api.Endpoints.Sales.Maps;

internal static class MapGetActivitySlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/activities", Handler).WithSummary("Get activities");
    }

    private static async Task<IResult> Handler(ISalesActivityQueryRepository repository, 
        AppUserAuthenticated user,
        Guid? opportunityId,
        Guid? companyId,
        int page = 1,
        int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new SalesActivityQuery(opportunityId, companyId, page, pageSize);
        var result = await repository.GetSlicesAsync(user.OrganizationId, query, ct);
        return TypedResults.Ok(result);
    }
    
}