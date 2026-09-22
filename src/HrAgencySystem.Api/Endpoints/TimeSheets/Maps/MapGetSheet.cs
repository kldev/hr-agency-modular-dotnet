using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapGetSheet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.TimeSheets.Get, Handler)
            .WithSummary("Get somebody's sheet for a month")
            .WithName("Get time sheet")
            .ProducesStandardErrors()
            .Produces<TimeSheetProjection>();
    }

    /// <summary>
    /// Reading somebody else's month is for the person above them or for payroll. Their own sheet
    /// goes through <c>/my</c>, so this one never has to decide whether to show a draft.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        [FromRoute] int year,
        [FromRoute] int month,
        ITimeSheetQueryRepository repository,
        IOrgStructureQueryRepository chart,
        CancellationToken ct
    )
    {
        if (userId != user.UserId && !PayrollPolicy.IsPayroll(user.Role))
        {
            var structure = await chart.GetStructureAsync(user.GetOrganization, ct);

            if (
                structure is null
                || !SupervisorPolicy.IsAbove(structure.AsUnits(), user.UserId, userId)
            )
                throw new OrganizationAccessDeniedException();
        }

        return TypedResults.Ok(
            await repository.GetAsync(user.GetOrganization, userId, year, month, ct)
        );
    }
}
