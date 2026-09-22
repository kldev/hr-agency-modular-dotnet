using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.FollowUpActions.Create;
using HrAgencySystem.Sales.Events.FollowUp;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SalesFollowUpAction.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // POST /api/sales/follow-up
        group
            .MapPost(ApiEndpoints.Sales.FollowUpActions.Create, Handler)
            .WithSummary("Create follow up action")
            .WithName("Create follow up action")
            .Produces<FollowUpActionCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        CreateFollowUpActionRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<FollowUpActionCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );

        return TypedResults.Created($"/api/sales/follow-up/{result.FollowUpActionId}", result);
    }

    internal sealed record CreateFollowUpActionRequest(
        [property: Description("The opportunity the follow-up is for.")] Guid OpportunityId,
        [property: Description("What is to be done, e.g. \"Send the revised offer\".")]
            string Content,
        [property: Description("When it is due, as a date and time with an offset (ISO 8601).")]
            DateTimeOffset FollowDateTime
    )
    {
        public CreateFollowUpAction ToCommand(Guid organizationId, Guid createdBy) =>
            new(organizationId, OpportunityId, Content, FollowDateTime, createdBy);
    }
}
