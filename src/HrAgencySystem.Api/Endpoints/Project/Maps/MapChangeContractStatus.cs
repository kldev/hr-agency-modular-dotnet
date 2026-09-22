using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Contract.ChangeStatus;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapChangeContractStatus
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/contract/status
        endpoints
            .MapPut(ApiEndpoints.Projects.ChangeContractStatus, Handler)
            .WithSummary("Change contract status")
            .WithName("Change contract status")
            .ProducesStandardErrors()
            .Produces<ProjectContractStatusChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        ChangeContractStatusRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectContractStatusChanged>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record ChangeContractStatusRequest(
        [property: Description("Draft, Signed, Terminated or Expired.")] ContractStatus Status,
        [property: Description(
            "Signature date, needed when the status becomes Signed. Omit it to keep the date already on the contract."
        )]
            DateOnly? SignedOn
    )
    {
        public ChangeContractStatus ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(projectId, organizationId.Value, Status, SignedOn, modifiedBy);
    }
}
