using HrAgencySystem.Agency.Application.Employment.ChangeTerms;
using HrAgencySystem.Agency.Application.Employment.Start;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.AgencyEmployment.Maps;

internal static class MapChangeTerms
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPut(ApiEndpoints.AgencyEmployments.Terms, Handler)
            .WithSummary("Change the terms somebody works on")
            .WithName("Change agency employment terms")
            .ProducesStandardErrors()
            .Produces<AgencyEmploymentTermsChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        ChangeAgencyEmploymentTermsRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<AgencyEmploymentTermsChanged>(
                request.ToCommand(
                    userId,
                    user.GetOrganization,
                    RatesPolicy.IsRates(user.Role),
                    user.UserId
                ),
                ct
            )
        );

    internal sealed record ChangeAgencyEmploymentTermsRequest(
        WorkerContractType ContractType,
        DateOnly EffectiveFrom,
        decimal? WeeklyHours,
        RateInput? Rate = null
    )
    {
        public ChangeAgencyEmploymentTerms ToCommand(
            Guid userId,
            OrganizationId organizationId,
            bool mayQuoteRate,
            Guid modifiedBy
        ) =>
            new(
                organizationId.Value,
                userId,
                ContractType,
                EffectiveFrom,
                WeeklyHours,
                Rate,
                mayQuoteRate,
                modifiedBy
            );
    }
}
