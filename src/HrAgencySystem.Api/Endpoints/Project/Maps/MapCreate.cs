using System.ComponentModel;
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
        [property: Description(
            "The client the project delivers for. Its profile has to be complete before the project can go live."
        )]
            Guid CompanyId,
        [property: Description(
            "Which of the agency's own companies delivers the project and employs or posts the people."
        )]
            Guid LegalEntityId,
        [property: Description("The project's name, e.g. \"Shipyard Gdansk - welders\".")]
            string Name,
        [property: Description("What is being delivered.")] string Description,
        [property: Description(
            "How the agency serves the client: PostingOfWorkers, TemporaryAgencyWork, Outsourcing or LocalEmployment. Together with the work country it decides which compliance requirements apply."
        )]
            EngagementType EngagementType,
        [property: Description(
            "Street of the workplace. Workplace address: street, building number, postal code, city and country of where the people work."
        )]
            string Street,
        [property: Description("Building number of the workplace.")] string BuildingNumber,
        [property: Description("Optional unit number of the workplace.")] string? UnitNumber,
        [property: Description("Postal code of the workplace.")] string PostalCode,
        [property: Description("City of the workplace.")] string City,
        [property: Description(
            "Country where the work happens, ISO 3166-1 alpha-2. It drives the compliance catalogue; Poland has no entries."
        )]
            string CountryCode,
        [property: Description("First day of the project.")] DateOnly StartsOn,
        [property: Description(
            "Last day of the project. Optional for an open-ended one; not before StartsOn."
        )]
            DateOnly? EndsOn,
        [property: Description("Optional recruitment team that staffs the project.")] Guid? TeamId
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
