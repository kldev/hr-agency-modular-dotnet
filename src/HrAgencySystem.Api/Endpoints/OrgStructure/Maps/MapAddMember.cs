using System.ComponentModel;
using HrAgencySystem.Agency.Application.OrgUnits.AddMember;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapAddMember
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.OrgStructure.Members, Handler)
            .WithSummary("Put somebody into a unit")
            .WithName("Add org unit member")
            .ProducesStandardErrors()
            .Produces<OrgUnitMemberAdded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid unitId,
        AddOrgUnitMemberRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitMemberAdded>(
                request.ToCommand(unitId, user.GetOrganization, user.UserId),
                ct
            )
        );

    /// <summary>
    /// <paramref name="Title"/> is optional and rarely used: it exists for the person the chart
    /// cannot otherwise describe, such as the second owner sitting on the board.
    /// </summary>
    internal sealed record AddOrgUnitMemberRequest(
        [property: Description(
            "The person to add. A person belongs to one unit at most - move them rather than add them twice."
        )]
            Guid UserId,
        [property: Description("Optional title within the unit, e.g. \"Payroll specialist\".")]
            string? Title
    )
    {
        public AddOrgUnitMember ToCommand(
            Guid unitId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(organizationId.Value, unitId, UserId, Title, modifiedBy);
    }
}
