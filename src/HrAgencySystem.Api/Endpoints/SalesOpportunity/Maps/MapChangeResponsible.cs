using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Sales.Application.Opportunities.ChangeResponsible;
using HrAgencySystem.Sales.Events.Opportunity;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;

internal static class MapChangeResponsible
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/sales/opportunity/{id}/responsible
        group.MapPut("{opportunityId:guid}/responsible", Handler)
            .WithSummary("Change responsible person")
            .Produces<ResponsiblePersonChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid opportunityId,
        ChangeResponsiblePersonRequest request,
        CancellationToken ct)
    {
        var command = new ChangeResponsiblePerson(opportunityId, 
            user.OrganizationId, 
            request.ResponsibleId, 
            user.UserId);
        
        var result = await bus.InvokeAsync<ResponsiblePersonChanged>(command, ct);
        
        return TypedResults.Ok(result);
    }
}

