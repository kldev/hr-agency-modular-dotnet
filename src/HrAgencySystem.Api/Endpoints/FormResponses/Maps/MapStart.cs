using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.Responses.Start;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.FormResponses.Maps;

internal static class MapStart
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/form-responses - for a one-per-person form a second call returns the first
        // response instead of failing.
        endpoints
            .MapPost(ApiEndpoints.FormResponses.Start, Handler)
            .WithSummary("Start filling in a form for a person")
            .WithName("Start form response")
            .ProducesStandardErrors()
            .Produces<FormResponseStarted>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        StartFormResponseRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormResponseStarted>(
                request.ToCommand(user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record StartFormResponseRequest(
        Guid FormId,
        [property: Description("Only 'worker' today.")] string SubjectKind,
        Guid SubjectId
    )
    {
        public StartFormResponse ToCommand(OrganizationId organizationId, Guid createdBy) =>
            new(organizationId.Value, FormId, SubjectKind, SubjectId, createdBy);
    }
}
