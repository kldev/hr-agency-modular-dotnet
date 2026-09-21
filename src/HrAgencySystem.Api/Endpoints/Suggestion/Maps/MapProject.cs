using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Suggestion;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapProject
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Project, Handler)
            .Produces<ProjectSuggestion>()
            .WithName("Get project suggestion")
            .WithSummary("Get a single project suggestion by id")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IProjectSuggestionRepository repository,
        Guid projectId,
        CancellationToken ct
    )
    {
        var result = await repository.ById(user.GetOrganization, projectId, ct);

        if (result is null)
            throw new NotFoundException("Project", projectId);

        return TypedResults.Ok(result);
    }
}
