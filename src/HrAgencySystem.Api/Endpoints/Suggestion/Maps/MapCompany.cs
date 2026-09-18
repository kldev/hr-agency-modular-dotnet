using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapCompany
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group
            .MapGet("/api/suggestion/companies/{companyId:guid}", Handler)
            .Produces<CompanySuggestion>()
            .WithName("Get company suggestion")
            .WithSummary("Get a single company suggestion by id")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ICompanySuggestionRepository repository,
        Guid companyId,
        CancellationToken ct
    )
    {
        var result = await repository.GetCompanySuggestion(user.OrganizationId, companyId, ct);

        if (result is null)
            throw new NotFoundException("Company", companyId);

        return TypedResults.Ok(result);
    }
}
