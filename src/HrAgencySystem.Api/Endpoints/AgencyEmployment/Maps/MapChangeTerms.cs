using System.ComponentModel;
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
        [property: Description(
            "The contract type from now on. Moving between types that do and do not owe hours adds the person to, or takes them off, the time sheet monitoring."
        )]
            WorkerContractType ContractType,
        [property: Description("When the new terms apply; not before the engagement began.")]
            DateOnly EffectiveFrom,
        [property: Description("Hours a week, 0 to 168. Optional.")] decimal? WeeklyHours,
        [property: Description(
            "Optional rate: amount, currency, unit (Hourly, Daily, Monthly) and basis (Gross, Net). Only HumanResources, Finance and Admin may set it - anyone else sending one is refused. The terms are replaced whole, so for those roles omitting it removes the rate; for anyone else the rate in force is kept."
        )]
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
