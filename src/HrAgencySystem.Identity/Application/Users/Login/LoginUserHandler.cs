using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Identity.Application.Users.Login;

public static class LoginUserHandler
{
    public static async Task<LoginUserResult> Handle(
        LoginUser command,
        ILogger logger,
        IPasswordHasher hasher,
        IAccountRepository repository,
        IJwtTokenService tokenService,
        IQueryOrganizationRepository queryOrganizationRepository,
        IRefreshTokenRepository refreshTokens,
        IOptions<JwtConfig> jwt,
        IClock clock,
        CancellationToken ct
    )
    {
        var email = Email.Create(command.Email);
        var reservation = await GetEmailReservation(
            command,
            repository,
            email,
            queryOrganizationRepository,
            ct
        );

        ValidatePassword(command, hasher, reservation);

        var user = await repository.GetUser(UserId.From(reservation.UserId), ct);

        var access = tokenService.GenerateUserToken(user);

        // A login opens a new family; nothing here touches the families of other devices, so signing
        // in on a second machine does not knock the first one out.
        var (refreshToken, refreshValue) = RefreshToken.Issue(
            user.Id,
            user.OrganizationId,
            clock,
            jwt.Value.RefreshTokenExpiresInDays
        );

        await refreshTokens.IssueAsync(refreshToken, ct);

        return new LoginUserResult(
            access.Value,
            refreshValue,
            access.ExpiresAt,
            refreshToken.ExpiresAt
        );
    }

    private static void ValidatePassword(
        LoginUser command,
        IPasswordHasher hasher,
        UserEmailReservation reservation
    )
    {
        var match = hasher.Matches(command.Password, reservation.PasswordHash);

        if (!match)
            throw new AuthorizationException("Invalid login or password");
    }

    private static async Task<UserEmailReservation> GetEmailReservation(
        LoginUser command,
        IAccountRepository repository,
        Email email,
        IQueryOrganizationRepository queryOrganizationRepository,
        CancellationToken ct
    )
    {
        var slug = command.Slug;
        if (string.IsNullOrEmpty(command.Slug))
        {
            var domain = email.Value.Split("@", StringSplitOptions.RemoveEmptyEntries)[1];
            var organization = await queryOrganizationRepository.GetByEmailDomainAsync(domain, ct);
            slug = organization?.Slug ?? "";
            if (string.IsNullOrEmpty(slug))
            {
                throw new NotFoundException("Organization by domain", domain);
            }
        }

        var reservation = await repository.FindUserByEmail(email, slug, ct);

        return reservation ?? throw new AuthorizationException("Invalid login or password");
    }
}
