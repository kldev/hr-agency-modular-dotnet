using HrAgencySystem.Agency.Application.TimeSheets.SaveWorkDay;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapSaveWorkDay
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPut(ApiEndpoints.TimeSheets.MyDays, Handler)
            .WithSummary("Record a day of my own work")
            .WithName("Save work day")
            .ProducesStandardErrors()
            .Produces<WorkDaySaved>();
    }

    /// <summary>
    /// Always the signed-in person. Filling hours in on somebody else's behalf is a separate
    /// decision with its own consequences - the entry would have to say who typed it - and is
    /// deliberately not offered here.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        SaveWorkDayRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<WorkDaySaved>(
                request.ToCommand(user.GetOrganization, user.UserId),
                ct
            )
        );

    /// <summary>
    /// Start plus length, not start and end. That is how people describe a day - "in at eight, did
    /// eight and a half" - and it spares them the arithmetic before they can type.
    /// </summary>
    internal sealed record SaveWorkDayRequest(
        DateOnly Date,
        TimeOnly StartsAt,
        int Hours,
        int Minutes,
        string? Note
    )
    {
        public SaveWorkDay ToCommand(OrganizationId organizationId, Guid userId) =>
            new(
                organizationId.Value,
                userId,
                Date.Year,
                Date.Month,
                Date,
                StartsAt,
                Hours,
                Minutes,
                Note,
                userId
            );
    }
}
