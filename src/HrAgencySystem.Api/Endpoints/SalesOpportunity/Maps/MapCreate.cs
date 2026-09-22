using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.Extensions;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        // /api/sales/opportunity
        group
            .MapPost(ApiEndpoints.Sales.Opportunities.Create, Handler)
            .WithSummary("Create opportunity")
            .WithName("Create opportunity")
            .Produces<OpportunityCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        CreateOpportunityRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<OpportunityCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );

        return TypedResults.Created($"/api/sales/opportunity/{result.OpportunityId}", result);
    }

    internal sealed record CreateOpportunityRequest(
        [property: Description(
            "The client company the opportunity is with. Must belong to the caller's agency."
        )]
            Guid CompanyId,
        [property: Description(
            "A short name for the deal, e.g. \"Five welders for the Gdansk shipyard\"."
        )]
            string Title,
        [property: Description(
            "What the client wants, in as much detail as is known. Up to 5000 characters."
        )]
            string Description,
        [property: Description("What the deal is expected to be worth, in Currency.")]
            decimal ExpectedValue,
        [property: Description(
            "Flag an opportunity that needs attention first. It only sorts and highlights; it changes no rule."
        )]
            bool IsHotLead,
        [property: Description("Currency of ExpectedValue, e.g. PLN, EUR.")] CurrencyCode Currency,
        [property: Description("When the deal is expected to be decided. Optional.")]
            DateOnly? ExpectedCloseDate,
        [property: Description(
            "The user who owns the opportunity. Omit to take it yourself; naming somebody else mails them about it."
        )]
            Guid? ResponsibleId
    )
    {
        public CreateOpportunity ToCommand(Guid organizationId, Guid createdBy) =>
            new(
                organizationId,
                CompanyId,
                Title,
                Description,
                ExpectedValue,
                IsHotLead,
                Currency,
                ExpectedCloseDate,
                ResponsibleId,
                createdBy
            );
    }
}
