using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Contract.Record;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapRecordContract
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/contract
        endpoints
            .MapPut(ApiEndpoints.Projects.RecordContract, Handler)
            .WithSummary("Record project contract")
            .WithName("Record project contract")
            .ProducesStandardErrors()
            .Produces<ProjectContractRecorded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        RecordProjectContractRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectContractRecorded>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// No party fields: the client on a contract is frozen from the company record at the moment it
    /// is recorded, never typed in, so that the two can never disagree.
    /// </summary>
    internal sealed record RecordProjectContractRequest(
        string ContractNumber,
        ContractStatus Status,
        DateOnly? SignedOn,
        DateOnly ValidFrom,
        DateOnly? ValidTo
    )
    {
        public RecordProjectContract ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) =>
            new(
                projectId,
                organizationId.Value,
                ContractNumber,
                Status,
                SignedOn,
                ValidFrom,
                ValidTo,
                modifiedBy
            );
    }
}
