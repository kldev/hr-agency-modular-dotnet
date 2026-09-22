using System.ComponentModel;
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
        [property: Description("The contract's number as written on it.")] string ContractNumber,
        [property: Description(
            "Draft, Signed, Terminated or Expired. A project goes live only with a Signed one."
        )]
            ContractStatus Status,
        [property: Description(
            "Signature date - required for a Signed contract, and not after ValidFrom."
        )]
            DateOnly? SignedOn,
        [property: Description("First day the contract applies.")] DateOnly ValidFrom,
        [property: Description("Last day it applies. Optional; not before ValidFrom.")]
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
