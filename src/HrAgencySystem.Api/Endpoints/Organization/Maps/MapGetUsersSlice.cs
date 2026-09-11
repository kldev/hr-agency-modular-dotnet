using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Organization.Maps;

internal static class MapGetUsersSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/organization/users
        group.MapGet("/users", Handler)
            .WithSummary("Get users")
            .Produces<SliceResponse<UserProjection>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(OwnerAuthenticated user,
        IUserQueryRepository repository,
        CancellationToken ct,
        [FromQuery] string? search,
        [FromQuery] OrganizationRole[] roles,
        Guid? organizationId,
        int page = 1, int pageSize = 100)
    {
        var result = await repository
            .GetUsersOwner(organizationId, search ?? "", roles ?? [], page, pageSize, ct);
        return TypedResults.Ok(result);
    }
}