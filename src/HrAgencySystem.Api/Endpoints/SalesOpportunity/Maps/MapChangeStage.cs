using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.Opportunities.ChangeStage;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;

internal static class MapChangeStage
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/sales/opportunity/{id}/stage
        group
            .MapPut(ApiEndpoints.Sales.Opportunities.ChangeStage, Handler)
            .WithSummary("Change stage")
            .WithName("Change opportunity stage")
            .Produces<StageChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid opportunityId,
        ChangeOpportunityStageRequest request,
        CancellationToken ct
    )
    {
        var command = new ChangeOpportunityStage(
            opportunityId,
            user.OrganizationId,
            request.Stage,
            request.LostReason ?? "",
            user.UserId
        );

        var result = await bus.InvokeAsync<StageChanged>(command, ct);

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal record ChangeOpportunityStageRequest(
    [property: Description(
        "The stage to move to: New, Viewed, Contacted, Qualified, Proposal, Won or Lost."
    )]
        OpportunityStage Stage,
    [property: Description(
        "Why the deal was lost - required when Stage is Lost, up to 500 characters, ignored otherwise."
    )]
        string? LostReason = null
);
