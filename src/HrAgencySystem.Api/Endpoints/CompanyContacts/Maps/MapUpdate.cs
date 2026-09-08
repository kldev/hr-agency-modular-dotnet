using HrAgencySystem.Api.Auth;
using HrAgencySystem.Company.Documents;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.CompanyContacts.Maps;


internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{contactId:guid}", Handler).WithSummary("Update contact");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IMessageBus bus,
        Guid contactId,
        CompanyContactRequest request,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<CompanyContact>(
                request.ToUpdateCommand(user.OrganizationId, contactId, user.UserId), 
                ct);
        return TypedResults.Ok(result);
    }
}

