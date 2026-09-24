using System.ComponentModel;
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
        group
            .MapPut(ApiEndpoints.Sales.Opportunities.Update, Handler)
            .WithSummary("Update opportunity")
            .WithName("Update opportunity")
            .Produces<OpportunityUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        Guid opportunityId,
        UpdateOpportunityRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<OpportunityUpdated>(
            request.ToCommand(user.OrganizationId, opportunityId, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record UpdateOpportunityRequest(
        [property: Description("A short name for the deal.")] string Title,
        [property: Description("What the client wants. Up to 5000 characters.")] string Description,
        [property: Description("What the deal is expected to be worth, in Currency.")]
            decimal ExpectedValue,
        [property: Description(
            "Flag an opportunity that needs attention first. It only sorts and highlights."
        )]
            bool IsHotLead,
        [property: Description("Currency of ExpectedValue, e.g. PLN, EUR.")] CurrencyCode Currency,
        [property: Description("When the deal is expected to be decided. Null clears it.")]
            DateOnly? ExpectedCloseDate
    )
    {
        public UpdateOpportunity ToCommand(
            Guid organizationId,
            Guid opportunityId,
            Guid modifiedBy
        ) =>
            new(
                opportunityId,
                organizationId,
                Title,
                Description,
                ExpectedValue,
                IsHotLead,
                Currency,
                ExpectedCloseDate,
                modifiedBy
            );
    }
}
