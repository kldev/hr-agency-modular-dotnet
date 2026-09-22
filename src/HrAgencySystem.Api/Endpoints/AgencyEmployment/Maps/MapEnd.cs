using System.ComponentModel;
using HrAgencySystem.Agency.Application.Employment.End;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.AgencyEmployment.Maps;

internal static class MapEnd
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.AgencyEmployments.End, Handler)
            .WithSummary("End somebody's engagement")
            .WithName("End agency employment")
            .ProducesStandardErrors()
            .Produces<AgencyEmploymentEnded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        EndAgencyEmploymentRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<AgencyEmploymentEnded>(
                request.ToCommand(userId, user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record EndAgencyEmploymentRequest(
        [property: Description(
            "Last day of the engagement; not before it began. An ended engagement cannot be changed any more."
        )]
            DateOnly EndsOn
    )
    {
        public EndAgencyEmployment ToCommand(
            Guid userId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(organizationId.Value, userId, EndsOn, modifiedBy);
    }
}
