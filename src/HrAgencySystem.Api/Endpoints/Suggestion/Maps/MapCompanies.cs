using HrAgencySystem.Api.Auth;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapCompanies
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group.MapGet("/api/suggestion/companies", Handler).WithSummary("Get top 25 companies");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, ICompanySuggestionRepository repository,
        CancellationToken ct,
        string? search, string? countryCode)
    {
        var result = await repository.GetCompanySuggestions(user.OrganizationId, search ?? "", countryCode ?? "", ct);
        return TypedResults.Ok(result);
    }
}