using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.FollowUpActions.Update;
using HrAgencySystem.Sales.Events.FollowUp;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SalesFollowUpAction.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/sales/follow-up/{followUpActionId}
        group.MapPut("{followUpActionId:guid}", Handler)
            .WithSummary("Update follow up action")
            .WithName("Update follow up action")
            .Produces<FollowUpActionUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid followUpActionId,
        UpdateFollowUpActionRequest request,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<FollowUpActionUpdated>(
            request.ToCommand(user.OrganizationId, followUpActionId, user.UserId), ct);

        return TypedResults.Ok(result);
    }

    internal sealed record UpdateFollowUpActionRequest(
        string Content,
        DateTimeOffset FollowDateTime)
    {
        public UpdateFollowUpAction ToCommand(Guid organizationId, Guid followUpActionId, Guid modifiedBy)
            => new(
                followUpActionId,
                organizationId,
                Content,
                FollowDateTime,
                modifiedBy);
    }
}
