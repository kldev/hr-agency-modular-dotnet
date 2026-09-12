using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Company.Application.Contacts.Delete;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.CompanyContacts.Maps;

internal static class MapDelete
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/recruitment/job-applications/{applicationId}/{noteId}/note
        group.MapDelete("{contactId:guid}", Handler)
            .WithSummary("Delete contact")
            .WithName("Delete company contact")
            .Produces<CompanyContactDeleted>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IMessageBus bus,
        Guid contactId,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<CompanyContactDeleted>(
                new DeleteCompanyContact(user.OrganizationId, contactId, user.UserId), ct);
        return TypedResults.Ok(result);
    }
}