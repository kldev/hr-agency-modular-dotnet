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
        group.MapPost("{companyId:guid}", Handler)
            .WithSummary("Create contact")
            .Produces<CompanyContact>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IMessageBus bus,
        Guid companyId,
        CompanyContactRequest request,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<CompanyContact>(
                request.ToCreateCommand(user.OrganizationId, companyId, user.UserId), 
                ct);
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CompanyContactRequest(
    ContactPerson Contact, bool UpdatePrimary = false)
{
    public CreateCompanyContact ToCreateCommand(Guid organizationId, Guid companyId, Guid createdBy)
    {
        return new CreateCompanyContact(organizationId, companyId,
            Contact, createdBy,  UpdatePrimary);
    }

    public UpdateCompanyContact ToUpdateCommand(Guid organizationId, Guid contactId, Guid modifiedBy)
    {
        return new UpdateCompanyContact(organizationId, contactId,
            Contact, UpdatePrimary, modifiedBy);
    }
}
