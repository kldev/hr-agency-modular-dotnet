using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.CompleteProfile;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Company.Maps;

internal static class MapCompleteProfile
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/companies/{companyId}/profile
        endpoints
            .MapPut(ApiEndpoints.Companies.CompleteProfile, Handler)
            .WithSummary("Complete company profile")
            .WithName("Complete company profile")
            .ProducesStandardErrors()
            .Produces<CompanyProfileUpdated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid companyId,
        CompleteCompanyProfileRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CompanyProfileUpdated>(
            request.ToCommand(
                companyId: companyId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Every field is optional: this is a form people fill in over several sittings, and the one
    /// place a company's paperwork is edited. Creating a lead never comes through here.
    /// </summary>
    internal sealed record CompleteCompanyProfileRequest(
        [property: Description(
            "The name as registered, the one a contract is signed with. With a registered address and a tax id it makes the profile complete - which a project needs before it can go live."
        )]
            string? LegalName,
        [property: Description(
            "Street of the registered address. Registered address: street, building number, postal code, city and country go together - all of them or none. Half an address is refused."
        )]
            string? Street,
        [property: Description(
            "Building number of the registered address. Registered address: street, building number, postal code, city and country go together - all of them or none. Half an address is refused."
        )]
            string? BuildingNumber,
        [property: Description("Optional flat or unit number of the registered address.")]
            string? UnitNumber,
        [property: Description(
            "Postal code of the registered address. Registered address: street, building number, postal code, city and country go together - all of them or none. Half an address is refused."
        )]
            string? PostalCode,
        [property: Description(
            "City of the registered address. Registered address: street, building number, postal code, city and country go together - all of them or none. Half an address is refused."
        )]
            string? City,
        [property: Description(
            "Country of the registered address, ISO 3166-1 alpha-2. Registered address: street, building number, postal code, city and country go together - all of them or none. Half an address is refused."
        )]
            string? CountryCode,
        [property: Description("EU VAT number, if the company has one, e.g. \"PL1234567890\".")]
            string? VatNumber,
        [property: Description("Bank account for invoices, as an IBAN.")] string? Iban,
        [property: Description("The bank's BIC/SWIFT code.")] string? Bic,
        [property: Description("The person who may sign for the company, e.g. a board member.")]
            ContactPerson? LegalRepresentative
    )
    {
        public CompleteCompanyProfile ToCommand(
            Guid companyId,
            OrganizationId organizationId,
            Guid modifiedBy
        )
        {
            return new CompleteCompanyProfile(
                companyId,
                organizationId.Value,
                LegalName,
                Street,
                BuildingNumber,
                UnitNumber,
                PostalCode,
                City,
                CountryCode,
                VatNumber,
                Iban,
                Bic,
                LegalRepresentative,
                modifiedBy
            );
        }
    }
}
