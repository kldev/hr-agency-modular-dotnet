using HrAgencySystem.Agency.Application.OrgUnits.Create;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapCreateUnit
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.OrgStructure.CreateUnit, Handler)
            .WithSummary("Create an organizational unit")
            .WithName("Create org unit")
            .ProducesStandardErrors()
            .Produces<OrgUnitCreated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        CreateOrgUnitRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<OrgUnitCreated>(
                request.ToCommand(user.GetOrganization, user.UserId),
                ct
            )
        );

    /// <summary>A null parent asks for the top unit, of which there is exactly one.</summary>
    internal sealed record CreateOrgUnitRequest(Guid? ParentId, string Name, OrgUnitKind Kind)
    {
        public CreateOrgUnit ToCommand(OrganizationId organizationId, Guid createdBy) =>
            new(organizationId.Value, ParentId, Name, Kind, createdBy);
    }
}
