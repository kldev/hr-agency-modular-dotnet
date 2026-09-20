using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Application.Create;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/projects
        endpoints
            .MapPost(ApiEndpoints.Projects.Create, Handler)
            .WithSummary("Create project")
            .WithName("Create project")
            .ProducesStandardErrors()
            .Produces<ProjectCreated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        CreateProjectRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectCreated>(
            request.ToCommand(organizationId: user.GetOrganization, createdBy: user.UserId),
            ct
        );

        return TypedResults.Created($"/api/projects/{result.ProjectId}", result);
    }

    internal sealed record CreateProjectRequest(
        Guid CompanyId,
        Guid LegalEntityId,
        string Name,
        string Description,
        EngagementType EngagementType,
        string Street,
        string BuildingNumber,
        string? UnitNumber,
        string PostalCode,
        string City,
        string CountryCode,
        DateOnly StartsOn,
        DateOnly? EndsOn,
        Guid? TeamId
    )
    {
        public CreateProject ToCommand(OrganizationId organizationId, Guid createdBy) =>
            new(
                organizationId.Value,
                CompanyId,
                LegalEntityId,
                Name,
                Description,
                EngagementType,
                Street,
                BuildingNumber,
                UnitNumber,
                PostalCode,
                City,
                CountryCode,
                StartsOn,
                EndsOn,
                TeamId,
                createdBy
            );
    }
}
