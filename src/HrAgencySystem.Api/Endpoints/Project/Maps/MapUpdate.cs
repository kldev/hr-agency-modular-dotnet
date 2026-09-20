using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Update;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}
        endpoints
            .MapPut(ApiEndpoints.Projects.Update, Handler)
            .WithSummary("Update project")
            .WithName("Update project")
            .ProducesStandardErrors()
            .Produces<ProjectUpdated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        UpdateProjectRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectUpdated>(
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
    /// No company and no engagement type: both are fixed at creation, the way a job description's
    /// company is. Moving a project to another client would silently invalidate its contract and
    /// every declaration filed for it.
    /// </summary>
    internal sealed record UpdateProjectRequest(
        string Name,
        string Description,
        string Street,
        string BuildingNumber,
        string? UnitNumber,
        string PostalCode,
        string City,
        string CountryCode,
        DateOnly StartsOn,
        DateOnly? EndsOn
    )
    {
        public UpdateProject ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) =>
            new(
                projectId,
                organizationId.Value,
                Name,
                Description,
                Street,
                BuildingNumber,
                UnitNumber,
                PostalCode,
                City,
                CountryCode,
                StartsOn,
                EndsOn,
                modifiedBy
            );
    }
}
