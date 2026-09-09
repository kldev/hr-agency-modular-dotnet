using HrAgencySystem.Api.Auth;
using HrAgencySystem.Sales.Application.Activity.Create;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Sales.Maps;

internal static class MapLogActivity
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/activity", Handler);
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        CreateSalesActivityRequest request, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<SalesActivityCreated>(request.ToCommand(user.OrganizationId, user.UserId), ct);
        
        return TypedResults.Created($"/api/sales/{result.SalesOpportunityId}/activities", result);
    }
    
}

// ReSharper disable once ClassNeverInstantiated.Global
internal record CreateSalesActivityRequest(
    Guid OpportunityId,
    SalesActivityType Type,
    string Note)
{
    public CreateSalesActivity ToCommand(Guid organizationId, Guid createdBy)
    {
        return new CreateSalesActivity(organizationId, OpportunityId, Type, Note, createdBy);

    }
}