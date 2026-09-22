using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Update;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Companies.Update, Handler)
            .WithSummary("Update company")
            .WithName("Update company")
            .Produces<CompanyUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid companyId,
        UpdateCompanyRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CompanyUpdated>(
            request.ToCommand(user.OrganizationId, companyId, user.UserId),
            ct
        );
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal record UpdateCompanyRequest(
    [property: Description("The name the agency knows the client by.")] string Name,
    [property: Description("Company register number (KRS or REGON in Poland).")]
        string RegistrationNumber,
    [property: Description("What the company does, e.g. Software, Manufacturing, Logistics.")]
        Industry Industry,
    [property: Description("The company's website address.")] string WebSite,
    [property: Description("Where the company is based, ISO 3166-1 alpha-2, e.g. \"PL\".")]
        string CountryCode
)
{
    public UpdateCompany ToCommand(Guid organizationId, Guid companyId, Guid modifiedBy) =>
        new(
            companyId,
            organizationId,
            Name,
            RegistrationNumber,
            Industry,
            WebSite,
            CountryCode,
            modifiedBy
        );
}
