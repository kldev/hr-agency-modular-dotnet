using HrAgencySystem.Api.Auth;
using HrAgencySystem.Company.Application.Contacts.Create;
using HrAgencySystem.Company.Application.Contacts.Update;
using HrAgencySystem.Company.Documents;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.CompanyContacts.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{companyId:guid}", Handler).WithSummary("Create contact");
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
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string JobTitle)
{
    public CreateCompanyContact ToCreateCommand(Guid organizationId, Guid companyId, Guid createdBy)
    {
        return new CreateCompanyContact(organizationId, companyId,
            Email, FirstName, LastName, JobTitle, Phone, createdBy);
    }
    
    public UpdateCompanyContact ToUpdateCommand(Guid organizationId, Guid contactId, Guid modifiedBy)
    {
        return new UpdateCompanyContact(organizationId, contactId,
            Email, FirstName, LastName, JobTitle, Phone, modifiedBy);
    }
}
