using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.Activities.Create;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Sales.Maps;

internal static class MapLogActivity
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Sales.LogActivity, Handler)
            .WithSummary("Log activity")
            .WithName("Log sales activity")
            .Produces<ActivityCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        CreateSalesActivityRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ActivityCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );

        return TypedResults.Created(
            $"/api/sales/activities?opportunityId={result.SalesOpportunityId}",
            result
        );
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal record CreateSalesActivityRequest(
    [property: Description("The opportunity the activity belongs to.")] Guid OpportunityId,
    [property: Description("What happened: Call, Email, Meeting, Note, Presentation or Other.")]
        SalesActivityType Type,
    [property: Description("Optional note on how it went, up to 500 characters.")] string Note
)
{
    public CreateActivity ToCommand(Guid organizationId, Guid createdBy)
    {
        return new CreateActivity(organizationId, OpportunityId, Type, Note, createdBy);
    }
}
