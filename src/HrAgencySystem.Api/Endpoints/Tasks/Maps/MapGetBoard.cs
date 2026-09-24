using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Tasks.Application.Port;
using HrAgencySystem.Tasks.Domain;

namespace HrAgencySystem.Api.Endpoints.Tasks.Maps;

internal static class MapGetBoard
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/tasks?range=week&timeZone=Europe/Warsaw - the caller's own tasks.
        group
            .MapGet(ApiEndpoints.Tasks.Board, Handler)
            .WithSummary("Get my tasks for a day, a week or a month")
            .WithDescription(
                "Open tasks due before the end of the range, overdue ones included, and tasks done within it. The range is worked out in the given IANA time zone (Europe/Warsaw when left out)."
            )
            .WithName("Get task board")
            .Produces<TaskBoard>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        ITaskItemsQueryRepository repository,
        AppUserAuthenticated user,
        IClock clock,
        [Description("Day, Week or Month.")] TaskRangeKind range = TaskRangeKind.Week,
        [Description("IANA time zone the range is counted in, e.g. Europe/Warsaw.")] string? timeZone = null,
        [Description("Only the tasks of this company.")] Guid? companyId = null,
        CancellationToken ct = default
    )
    {
        var now = clock.UtcNow;
        var query = new TaskBoardQuery(
            user.UserId,
            TaskRange.Resolve(range, timeZone, now),
            now,
            companyId
        );

        return TypedResults.Ok(await repository.GetBoard(user.GetOrganization, query, ct));
    }
}
