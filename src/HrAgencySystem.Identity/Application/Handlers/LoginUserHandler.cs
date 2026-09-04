using HrAgencySystem.Identity.Application.Commands;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Persistence;
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
        var reservation = await GetEmailReservation(command, repository, email, ct);

        ValidatePassword(command, hasher, reservation);

        var user = await repository.GetUser(UserId.From(reservation.UserId), ct);

        var token = tokenService.GenerateUserToken(user);

        return new LoginUserResult(token);
    }

    private static void ValidatePassword(LoginUser command, IPasswordHasher hasher, UserEmailReservation reservation)
    {
        var match = hasher.Matches(command.Password, reservation.PasswordHash);

        if (!match)
            throw new AuthorizationException("Invalid login or password");
    }

    private static async Task<UserEmailReservation> GetEmailReservation(LoginUser command, IAccountRepository repository, Email email, CancellationToken ct)
    {
        var reservation = await repository.FindUserByEmail(email, command.Slug, ct);

        return reservation ?? throw new AuthorizationException("Invalid login or password");
    }
}