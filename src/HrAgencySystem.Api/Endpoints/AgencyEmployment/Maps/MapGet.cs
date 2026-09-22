using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.AgencyEmployment.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.AgencyEmployments.Get, Handler)
            .WithSummary("Get somebody's employment record")
            .WithName("Get agency employment")
            .ProducesStandardErrors()
            .Produces<AgencyEmploymentProjection>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        IAgencyEmploymentQueryRepository repository,
        CancellationToken ct
    )
    {
        var employment =
            await repository.GetAsync(user.GetOrganization, userId, ct)
            ?? throw new NotFoundException("AgencyEmployment", userId);

        return TypedResults.Ok(RatesPolicy.IsRates(user.Role) ? employment : employment.WithoutRate());
    }
}
