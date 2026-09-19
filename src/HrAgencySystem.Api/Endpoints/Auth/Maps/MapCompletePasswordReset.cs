using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Sagas;
using HrAgencySystem.SharedKernel.Exception;
using Wolverine;
using Wolverine.Persistence.Sagas;

namespace HrAgencySystem.Api.Endpoints.Auth.Maps;

internal static class MapCompletePasswordReset
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Auth.CompletePasswordReset, Handler)
            .WithSummary("Set a new password from a reset link")
            .WithName("Complete password reset")
            .ProducesStandardErrors()
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        CompletePasswordResetRequest request
    )
    {
        try
        {
            await bus.InvokeAsync(request.ToCommand());
        }
        catch (UnknownSagaException)
        {
            // A window that already closed and a link that never existed have to look the same
            // from the outside, so this is the answer a wrong token gets as well.
            throw new BusinessRuleException(PasswordResetSaga.InvalidTokenMessage);
        }

        return TypedResults.NoContent();
    }

    internal record CompletePasswordResetRequest(Guid Id, string Token, string NewPassword)
    {
        public CompletePasswordReset ToCommand() => new(Id, Token, NewPassword);
    }
}
