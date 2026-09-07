using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Identity.Application.Users.Login;

public static class LoginUserHandler
{
    public static async Task<LoginUserResult> Handle(LoginUser command,
        ILogger logger,
        IPasswordHasher hasher,
        IAccountRepository repository,
        IJwtTokenService tokenService,
        IOrganizationService organizationService,
        CancellationToken ct)
    {

        var email = Email.Create(command.Email);
        var reservation = await GetEmailReservation(command, repository, email, organizationService, ct);

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

    private static async Task<UserEmailReservation> GetEmailReservation(LoginUser command, IAccountRepository repository, Email email, IOrganizationService organizationService, CancellationToken ct)
    {
        var slug = command.Slug;
        if (string.IsNullOrEmpty(command.Slug))
        {
            var domain = email.Value.Split("@", StringSplitOptions.RemoveEmptyEntries)[1];
            var organization = await organizationService.GetByEmailDomainAsync(domain, ct);
            slug = organization?.Slug ?? "";
        }
        
        var reservation = await repository.FindUserByEmail(email, slug, ct);

        return reservation ?? throw new AuthorizationException("Invalid login or password");
    }
}