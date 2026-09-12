using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Documents;

namespace HrAgencySystem.Api.Endpoints.CompanyContacts.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{contactId:guid}", Handler)
            .WithSummary("Get contact")
            .WithName("Get company contact")
            .Produces<CompanyContact>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        ICompanyContactQueryRepository repository,
        Guid contactId,
        CancellationToken ct)
    {
        var result = 
            await repository.GetAsync(user.OrganizationId, contactId, ct);

        if (result == null) return TypedResults.NotFound(DomainObjectNotFound.NotFound("Contact", contactId));
        
        return TypedResults.Ok(result);
    }
}