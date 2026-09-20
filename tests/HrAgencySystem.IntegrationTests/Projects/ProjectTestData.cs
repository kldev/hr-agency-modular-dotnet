using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Web.Common;
using CompanyMaps = HrAgencySystem.Api.Endpoints.Company.Maps;

namespace HrAgencySystem.IntegrationTests.Projects;

internal static class ProjectTestData
{
    public static readonly DateOnly StartsOn = new(2026, 10, 1);
    public static readonly DateOnly SignedOn = new(2026, 9, 15);

    public static MapCreate.CreateProjectRequest CreateRequest(
        Guid companyId,
        Guid legalEntityId,
        string name = "Delivery for ACME",
        EngagementType engagementType = EngagementType.TemporaryAgencyWork,
        string countryCode = "be",
        Guid? teamId = null
    ) =>
        new(
            companyId,
            legalEntityId,
            name,
            "Two developers on site.",
            engagementType,
            "Rue de la Loi",
            "16",
            null,
            "1000",
            "Bruxelles",
            countryCode,
            StartsOn,
            null,
            teamId
        );

    public static MapUpdate.UpdateProjectRequest UpdateRequest(
        string name = "Delivery for ACME - phase two"
    ) =>
        new(
            name,
            "Three developers on site.",
            "Rue de la Loi",
            "18",
            "4",
            "1000",
            "Bruxelles",
            "be",
            StartsOn,
            StartsOn.AddMonths(6)
        );

    /// <summary>The paperwork a company needs before it can be a party to a contract.</summary>
    public static CompanyMaps.MapCompleteProfile.CompleteCompanyProfileRequest CompanyProfile() =>
        new(
            "ACME Corporation sp. z o.o.",
            "Prosta",
            "51",
            null,
            "00-838",
            "Warszawa",
            "pl",
            "PL1234567890",
            null,
            null,
            null
        );

    public static MapAssignContact.AssignProjectContactRequest ContactRequest(
        string firstName = "Marie",
        string email = "marie@acme.example.com"
    ) =>
        new(
            new ContactPerson(email, firstName, "Dubois", "Operations lead", "+32 2 000 00 00"),
            null
        );

    public static MapRecordContract.RecordProjectContractRequest ContractRequest(
        string number = "UM/2026/17",
        ContractStatus status = ContractStatus.Signed
    ) => new(number, status, status is ContractStatus.Signed ? SignedOn : null, StartsOn, null);
}
