using System.ComponentModel;
using HrAgencySystem.Agency.Application.Employment.Start;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.AgencyEmployment.Maps;

internal static class MapStart
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.AgencyEmployments.Start, Handler)
            .WithSummary("Record that somebody works for the agency")
            .WithName("Start agency employment")
            .ProducesStandardErrors()
            .Produces<AgencyEmploymentStarted>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        StartAgencyEmploymentRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<AgencyEmploymentStarted>(
                request.ToCommand(
                    user.GetOrganization,
                    RatesPolicy.IsRates(user.Role),
                    user.UserId
                ),
                ct
            )
        );

    internal sealed record StartAgencyEmploymentRequest(
        [property: Description("The agency's own person the record is for. One record per person.")]
            Guid UserId,
        [property: Description(
            "EmploymentContract, TemporaryEmploymentContract, MandateContract, SelfEmployed or Other. Employment and mandate contracts owe a monthly time sheet; the others do not."
        )]
            WorkerContractType ContractType,
        [property: Description("First day of the engagement.")] DateOnly StartsOn,
        [property: Description("Hours a week, 0 to 168. Optional.")] decimal? WeeklyHours,
        [property: Description(
            "Optional rate: amount, currency, unit (Hourly, Daily, Monthly) and basis (Gross, Net). Only HumanResources, Finance and Admin may set it - anyone else sending one is refused."
        )]
            RateInput? Rate = null
    )
    {
        public StartAgencyEmployment ToCommand(
            OrganizationId organizationId,
            bool mayQuoteRate,
            Guid startedBy
        ) =>
            new(
                organizationId.Value,
                UserId,
                ContractType,
                StartsOn,
                WeeklyHours,
                Rate,
                mayQuoteRate,
                startedBy
            );
    }
}
