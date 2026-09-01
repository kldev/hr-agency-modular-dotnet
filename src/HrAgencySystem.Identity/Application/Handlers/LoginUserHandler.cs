using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Identity.Application.Handlers;

public static class LoginUserHandler
{
    public static async Task<LoginUserResult> Handle(LoginUser command,
        ILogger logger,
        IPasswordHasher hasher,
        IAccountRepository repository,
        IJwtTokenService tokenService,
        CancellationToken ct)
    {

        var email = Email.Create(command.Email);
        var reservation = await repository.FindUserByEmail(email, command.Slug, ct);

        if (reservation is null)
            throw new AuthorizationException("Invalid login or password");

        var match = hasher.Matches(command.Password, reservation.PasswordHash);

        if (!match)
            throw new AuthorizationException("Invalid login or password");

        var user = await repository.GetUser(UserId.From(reservation.UserId), ct);

        var token = tokenService.GenerateUserToken(user);

        return new LoginUserResult(token);
    }
}