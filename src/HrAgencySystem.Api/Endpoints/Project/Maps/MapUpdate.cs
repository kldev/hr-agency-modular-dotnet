using System.ComponentModel;
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
        [property: Description("The project's name.")] string Name,
        [property: Description("What is being delivered.")] string Description,
        [property: Description(
            "Street of the workplace. Workplace address: street, building number, postal code, city and country of where the people work."
        )]
            string Street,
        [property: Description("Building number of the workplace.")] string BuildingNumber,
        [property: Description("Optional unit number of the workplace.")] string? UnitNumber,
        [property: Description("Postal code of the workplace.")] string PostalCode,
        [property: Description("City of the workplace.")] string City,
        [property: Description("Country where the work happens, ISO 3166-1 alpha-2.")]
            string CountryCode,
        [property: Description("First day of the project.")] DateOnly StartsOn,
        [property: Description("Last day of the project, or null for open-ended.")] DateOnly? EndsOn
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
