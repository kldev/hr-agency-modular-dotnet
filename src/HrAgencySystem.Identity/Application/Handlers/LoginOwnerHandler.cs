using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Identity.Application.Handlers;

public static class LoginOwnerHandler
{
    public static async Task<LoginOwnerResult> Handle(LoginOwner command,
        ILogger logger,
        IPasswordHasher hasher,
        IAccountRepository repository,
        IJwtTokenService tokenService,
        CancellationToken ct)
    {
        var email = Email.Create(command.Email);
        var reservation = await repository.FindOwnerByEmail(email, ct);

        if (reservation is null)
            throw new AuthorizationException("Invalid login or password");

        var match = hasher.Matches(command.Password, reservation.PasswordHash);

        if (!match)
            throw new AuthorizationException("Invalid login or password");

        var user = await repository.GetOwner(PlatformOwnerId.From(reservation.OwnerId), ct);

        var token = tokenService.GenerateOwnerToken(user);

        return new LoginOwnerResult(token);
    }
}