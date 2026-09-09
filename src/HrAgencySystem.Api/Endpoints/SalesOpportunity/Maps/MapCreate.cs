using HrAgencySystem.Api.Auth;
using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/sales/opportunity
        group.MapPost("", Handler).WithSummary("Create opportunity");
    }

    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user,
        CreateOpportunityRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<OpportunityCreated>(request.ToCommand(user.OrganizationId, user.UserId), ct);

        return TypedResults.Created($"/api/sales/opportunity/{result.OpportunityId}", result);
    }

    internal sealed record CreateOpportunityRequest(
        Guid CompanyId,
        string Title,
        string Description,
        decimal ExpectedValue,
        CurrencyCode Currency,
        DateTimeOffset? ExpectedCloseDate,
        Guid? OwnerId)
    {
        public CreateOpportunity ToCommand(Guid organizationId, Guid createdBy)
            => new (organizationId, 
                CompanyId, 
                Title, 
                Description, 
                ExpectedValue, 
                Currency, 
                ExpectedCloseDate, 
                OwnerId, 
                createdBy);
    }
}