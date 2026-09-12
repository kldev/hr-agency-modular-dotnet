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
        group.MapPut("{companyId:guid}", Handler)
            .WithSummary("Update company")
            .WithName("Update company")
            .Produces<CompanyUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IMessageBus bus,
        Guid companyId,
        UpdateCompanyRequest request,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<CompanyUpdated>(
                request.ToCommand(user.OrganizationId, companyId, user.UserId), 
                ct);
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal record UpdateCompanyRequest(  
    string Name,
    string RegistrationNumber,
    Industry Industry,
    string WebSite,
    string CountryCode)
{
    public UpdateCompany ToCommand(Guid organizationId, Guid companyId, Guid modifiedBy)
        => new (companyId, 
            organizationId, 
            Name, 
            RegistrationNumber, 
            Industry, 
            WebSite, 
            CountryCode,
            modifiedBy);
}

