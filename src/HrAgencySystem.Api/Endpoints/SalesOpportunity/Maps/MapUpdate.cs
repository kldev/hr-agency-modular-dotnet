using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.Opportunities.Update;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/sales/opportunity/{id}
        group.MapPut("{opportunityId:guid}", Handler)
            .WithSummary("Update opportunity")
            .WithName("Update opportunity")
            .Produces<OpportunityUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IMessageBus bus,
        AppUserAuthenticated user,
        Guid opportunityId, UpdateOpportunityRequest request, CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<OpportunityUpdated>(
                request.ToCommand(user.OrganizationId, opportunityId, user.UserId), ct);

        return TypedResults.Ok(result);
    }
    
    
    internal sealed record UpdateOpportunityRequest(
        string Title,
        string Description,
        decimal ExpectedValue,
        bool IsHotLead,
        CurrencyCode Currency,
        DateTimeOffset? ExpectedCloseDate
        )
    {
        public UpdateOpportunity ToCommand(Guid organizationId, Guid opportunityId, Guid modifiedBy)
            => new (
                opportunityId,
                organizationId,
                Title, 
                Description, 
                ExpectedValue, 
                IsHotLead,
                Currency, 
                ExpectedCloseDate, 
                modifiedBy);
    }
}