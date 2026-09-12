using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapGetContacts
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{companyId:guid}/contacts", Handler)
            .WithSummary("Get contacts")
            .WithName("Get company contacts")
            .Produces<IReadOnlyList<CompanyContact>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        ICompanyContactQueryRepository repository,
        Guid companyId,
        CancellationToken ct)
    {
        var result = 
            await repository.GetAllAsync(user.OrganizationId, companyId, ct);
        return TypedResults.Ok(result);
    }
}