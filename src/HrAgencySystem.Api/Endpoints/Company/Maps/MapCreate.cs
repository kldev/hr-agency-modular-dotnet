using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Create;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/companies
        endpoints
            .MapPost(ApiEndpoints.Companies.Create, Handler)
            .WithSummary("Create company")
            .WithName("Create company")
            .ProducesStandardErrors()
            .Produces<CompanyCreated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        CreateCompanyRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CompanyCreated>(
            request.ToCommand(user.GetOrganization, user.UserId),
            ct
        );

        return TypedResults.Created($"/api/companies/{result.CompanyId}", result);
    }

    internal record CreateCompanyRequest(
        [property: Description(
            "The name the agency knows the client by, e.g. \"Cassin Inc\". The legal name belongs to the profile."
        )]
            string Name,
        [property: Description("Where the company is based, ISO 3166-1 alpha-2, e.g. \"PL\".")]
            string CountryCode,
        [property: Description(
            "Tax identification number (NIP in Poland). Unique within the agency - a second company with the same one is refused."
        )]
            string TaxId,
        [property: Description(
            "Company register number (KRS or REGON in Poland), as the client gives it."
        )]
            string RegistrationNumber,
        [property: Description("The company's website address.")] string Website,
        [property: Description(
            "What the company does, e.g. Software, Manufacturing, Logistics. Used to filter and group clients."
        )]
            Industry Industry,
        [property: Description(
            "Optional first contact person, recorded as the company's primary contact."
        )]
            ContactPerson? Contact = null
    )
    {
        public CreateCompany ToCommand(OrganizationId organizationId, Guid createdBy)
        {
            return new CreateCompany(
                organizationId.Value,
                Name,
                CountryCode,
                TaxId,
                RegistrationNumber,
                createdBy,
                Industry,
                Website,
                Contact
            );
        }
    }
}
