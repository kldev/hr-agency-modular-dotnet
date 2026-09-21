using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Positions.Update;
using HrAgencySystem.Projects.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Position.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/positions/{positionId}
        endpoints
            .MapPut(ApiEndpoints.Projects.UpdatePosition, Handler)
            .WithSummary("Update a position")
            .WithName("Update position")
            .ProducesStandardErrors()
            .Produces<ProjectPositionUpdated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] Guid positionId,
        PositionRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var open = request.ToOpenCommand(projectId, user.GetOrganization, user.UserId);

        var command = new UpdatePosition(
            open.ProjectId,
            open.OrganizationId,
            positionId,
            open.Name,
            open.ContractName,
            open.WorkDescription,
            open.Duties,
            open.RequiredQualifications,
            open.ContractType,
            open.RateAmount,
            open.RateCurrency,
            open.RateUnit,
            open.RateBasis,
            open.Street,
            open.BuildingNumber,
            open.UnitNumber,
            open.PostalCode,
            open.City,
            open.CountryCode,
            open.WeeklyHours,
            open.WorkStartsAt,
            open.WorkSchedule,
            open.PayoutDay,
            open.ProbationPeriod,
            open.NoticePeriod,
            open.Allowances,
            open.PlannedHeadcount,
            open.DefaultEngagementType,
            open.ModifiedBy
        );

        return TypedResults.Ok(await bus.InvokeAsync<ProjectPositionUpdated>(command, ct));
    }
}
