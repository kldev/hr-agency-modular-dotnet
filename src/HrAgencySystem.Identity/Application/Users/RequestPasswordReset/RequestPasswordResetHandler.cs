using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Configuration;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.Identity.Sagas;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;

namespace HrAgencySystem.Identity.Application.Users.RequestPasswordReset;

public static class RequestPasswordResetHandler
{
    public static async Task<(RequestPasswordResetResult, OutgoingMessages)> Handle(
        RequestPasswordReset command,
        IAccountRepository accounts,
        IQueryOrganizationRepository organizations,
        IOptions<IdentityConfig> config,
        IClock clock,
        ILogger logger,
        CancellationToken ct
    )
    {
        var messages = new OutgoingMessages();
        var email = Email.Create(command.Email);

        var user = await FindUser(command, accounts, organizations, email, ct);

        // An unknown address gets the same empty answer as a known one - anything else turns this
        // endpoint into a list of everybody who has an account here.
        if (user is null)
        {
            logger.LogInformation("Password reset requested for an unknown address, no mail sent");

            return (new RequestPasswordResetResult(clock.UtcNow), messages);
        }

        var resetId = Guid.NewGuid();
        var token = SecureToken.New();

        // The mail is the saga's to send: it is the thing that knows the window is open.
        messages.Add(
            new StartPasswordReset(
                resetId,
                user.Id,
                user.OrganizationId,
                user.Email,
                $"{user.FirstName} {user.LastName}",
                PasswordResetSaga.Hash(token),
                $"{command.PortalUrl.TrimEnd('/')}/reset-password?id={resetId}&token={token}",
                config.Value.PasswordResetExpiresInMinutes
            )
        );

        return (new RequestPasswordResetResult(clock.UtcNow), messages);
    }

    private static async Task<UserProjection?> FindUser(
        RequestPasswordReset command,
        IAccountRepository accounts,
        IQueryOrganizationRepository organizations,
        Email email,
        CancellationToken ct
    )
    {
        var slug = command.Slug;

        if (string.IsNullOrEmpty(slug))
        {
            var domain = email.Value.Split("@", StringSplitOptions.RemoveEmptyEntries)[1];
            var organization = await organizations.GetByEmailDomainAsync(domain, ct);

            slug = organization?.Slug ?? "";

            if (string.IsNullOrEmpty(slug))
                return null;
        }

        var reservation = await accounts.FindUserByEmail(email, slug, ct);

        return reservation is null
            ? null
            : await accounts.GetUser(UserId.From(reservation.UserId), ct);
    }
}
