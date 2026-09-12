using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Interviews.ChangeFormat;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Events.Interviews;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Interviews.Maps;


internal static class MapChangeFormat
{
    // PUT /api/interviews/{id}/format
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{interviewId}/format", Handler)
            .WithSummary("Change format")
            .WithName("Change interview format")
            .Produces<InterviewFormatChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user, Guid interviewId,
        ChangeInterviewFormatRequest request)
    {
        var result =
            await bus.InvokeAsync<InterviewFormatChanged>(request.ToCommand(interviewId,
                user.OrganizationId, user.UserId));

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangeInterviewFormatRequest(
    InterviewFormat Format,
    string? Note
)
{
    public ChangeInterviewFormat ToCommand(Guid interviewId, Guid organizationId, Guid userId)
        => new (interviewId, organizationId, Note ?? "", Format,  userId);
}