using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Config;
using HrAgencySystem.Identity.Application.Users.RequestPasswordReset;
using Microsoft.Extensions.Options;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapRequestPasswordReset
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Auth.RequestPasswordReset, Handler)
            .WithSummary("Request a password reset link")
            .WithName("Request password reset")
            .ProducesStandardErrors()
            .AllowAnonymous();
    }

    /// <summary>
    /// Always 202, whether or not the address belongs to anybody: the response is not allowed to be
    /// a way of asking which emails have an account here.
    /// </summary>
    private static async Task<IResult> Handler(
        IMessageBus bus,
        IOptions<ApplicationConfig> config,
        RequestPasswordResetRequest request
    )
    {
        await bus.InvokeAsync<RequestPasswordResetResult>(
            request.ToCommand(config.Value.PortalUrl)
        );

        return TypedResults.Accepted((string?)null);
    }

    internal record RequestPasswordResetRequest(string Email, string Slug = "")
    {
        public RequestPasswordReset ToCommand(string portalUrl) => new(Email, Slug, portalUrl);
    }
}
