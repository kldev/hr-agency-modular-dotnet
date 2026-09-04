using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Persistence;
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
        var reservation = await FindEmailReservation(repository, ct, email);

        ValidatePassword(command, hasher, reservation);

        var user = await repository.GetOwner(PlatformOwnerId.From(reservation.OwnerId), ct);

        var token = tokenService.GenerateOwnerToken(user);

        return new LoginOwnerResult(token);
    }

    private static void ValidatePassword(LoginOwner command, IPasswordHasher hasher, OwnerEmailReservation reservation)
    {
        var match = hasher.Matches(command.Password, reservation.PasswordHash);

        if (!match)
            throw new AuthorizationException("Invalid login or password");
    }

    private static async Task<OwnerEmailReservation> FindEmailReservation(IAccountRepository repository, CancellationToken ct, Email email)
    {
        var reservation = await repository.FindOwnerByEmail(email, ct);

        return reservation ?? throw new AuthorizationException("Invalid login or password");
    }
}