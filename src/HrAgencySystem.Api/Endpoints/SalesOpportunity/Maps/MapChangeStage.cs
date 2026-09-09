using HrAgencySystem.Api.Auth;
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
        group.MapPut("{opportunityId:guid}/stage", Handler).WithSummary("Change stage");
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid opportunityId,
        ChangeOpportunityStageRequest request,
        CancellationToken ct)
    {
        var command = new ChangeOpportunityStage(
            opportunityId,
            user.OrganizationId, 
            request.Stage, 
            request.LostReason ?? "",
            user.UserId);
        
        var result = await bus.InvokeAsync<StageChanged>(command, ct);
        
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal record ChangeOpportunityStageRequest(OpportunityStage Stage, string? LostReason = null);