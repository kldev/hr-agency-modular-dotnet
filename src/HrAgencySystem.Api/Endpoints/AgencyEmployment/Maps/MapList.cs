using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.AgencyEmployment.Maps;

internal static class MapList
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.AgencyEmployments.List, Handler)
            .WithSummary("List employment records")
            .WithName("List agency employments")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<AgencyEmploymentProjection>>();
    }

    /// <summary>
    /// With a year and a month, only the people the hours register actually covers in that month -
    /// which is the question the monitoring screen is really asking.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IAgencyEmploymentQueryRepository repository,
        CancellationToken ct,
        [FromQuery] int? year,
        [FromQuery] int? month
    )
    {
        var employments =
            year is null || month is null
                ? await repository.GetAllAsync(user.GetOrganization, ct)
                : await repository.GetCoveredAsync(
                    user.GetOrganization,
                    year.Value,
                    month.Value,
                    ct
                );

        return TypedResults.Ok(
            RatesPolicy.IsRates(user.Role)
                ? employments
                : [.. employments.Select(employment => employment.WithoutRate())]
        );
    }
}
