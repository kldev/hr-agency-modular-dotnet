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
        Guid UserId,
        WorkerContractType ContractType,
        DateOnly StartsOn,
        decimal? WeeklyHours,
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
