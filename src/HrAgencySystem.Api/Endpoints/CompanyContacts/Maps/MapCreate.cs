using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Contacts.Create;
using HrAgencySystem.Company.Application.Contacts.Update;
using HrAgencySystem.Company.Documents;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.CompanyContacts.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.CompanyContacts.Create, Handler)
            .WithSummary("Create contact")
            .WithName("Create company contact")
            .Produces<CompanyContact>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        Guid companyId,
        CompanyContactRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CompanyContact>(
            request.ToCreateCommand(user.OrganizationId, companyId, user.UserId),
            ct
        );
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CompanyContactRequest(
    [property: Description("The person: e-mail, first and last name, job title and phone.")]
        ContactPerson Contact,
    [property: Description(
        "Also make this person the company's primary contact. Applied when a contact is edited; creating a contact does not look at it today."
    )]
        bool UpdatePrimary = false
)
{
    public CreateCompanyContact ToCreateCommand(Guid organizationId, Guid companyId, Guid createdBy)
    {
        return new CreateCompanyContact(
            organizationId,
            companyId,
            Contact,
            createdBy,
            UpdatePrimary
        );
    }

    public UpdateCompanyContact ToUpdateCommand(
        Guid organizationId,
        Guid contactId,
        Guid modifiedBy
    )
    {
        return new UpdateCompanyContact(
            organizationId,
            contactId,
            Contact,
            UpdatePrimary,
            modifiedBy
        );
    }
}
