using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapUser
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Suggestions.User, Handler)
            .Produces<UserSuggestion>()
            .WithName("Get user suggestion")
            .WithSummary("Get a single user suggestion by id")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IUserSuggestionRepository repository,
        Guid userId,
        CancellationToken ct
    )
    {
        var result = await repository.GetUserSuggestion(user.GetOrganization, userId, ct);

        if (result is null)
            throw new NotFoundException("User", userId);

        return TypedResults.Ok(result);
    }
}
