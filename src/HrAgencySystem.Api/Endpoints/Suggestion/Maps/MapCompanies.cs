using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Suggestion;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapCompanies
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group.MapGet("/api/suggestion/companies", Handler)
            .Produces<IReadOnlyList<CompanySuggestion>>()
            .WithSummary("Get top 25 companies")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, ICompanySuggestionRepository repository,
        CancellationToken ct,
        string? search, string? countryCode)
    {
        var result = await repository.GetCompanySuggestions(user.OrganizationId, search ?? "", countryCode ?? "", ct);
        return TypedResults.Ok(result);
    }
}