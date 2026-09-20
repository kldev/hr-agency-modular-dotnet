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
        string? LegalName,
        string? Street,
        string? BuildingNumber,
        string? UnitNumber,
        string? PostalCode,
        string? City,
        string? CountryCode,
        string? VatNumber,
        string? Iban,
        string? Bic,
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
