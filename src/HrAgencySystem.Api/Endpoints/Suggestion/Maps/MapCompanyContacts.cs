using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.Company.Documents;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;


internal static class MapCompanyContacts
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group.MapGet("/api/suggestion/company-contacts", Handler)
            .WithSummary("Get top 25 contacts")
            .Produces<IReadOnlyList<CompanyContact>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, ICompanyContactSuggestionRepository repository,
        CancellationToken ct,
        string? search, Guid? companyId)
    {
        var result = await repository.GetSuggestionAsync(user.OrganizationId, search ?? "", companyId, ct);
        return TypedResults.Ok(result);
    }
}