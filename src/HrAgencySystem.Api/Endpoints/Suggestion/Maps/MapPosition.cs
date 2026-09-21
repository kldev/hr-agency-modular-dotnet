using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Suggestion;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapPosition
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.Position, Handler)
            .Produces<PositionSuggestion>()
            .WithName("Get position suggestion")
            .WithSummary("Get a single position suggestion by id")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IPositionSuggestionRepository repository,
        Guid positionId,
        CancellationToken ct
    )
    {
        var result = await repository.ById(user.GetOrganization, positionId, ct);

        if (result is null)
            throw new NotFoundException("Position", positionId);

        return TypedResults.Ok(result);
    }
}
