using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Factories;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Teams.Contracts;
using HrAgencySystem.Teams.Contracts.IntegrationCommands;
using Marten;
using Wolverine;

namespace HrAgencySystem.Identity.Application.Users.Create;

public static class CreateUserHandler
{
    public const string UserWithEmailMessage =
        "A user with this email already exists in the organization.";

    public const string TeamAndRoleTogetherMessage =
        "A team and a team role have to be given together.";

    public static async Task<(UserCreated, OutgoingMessages)> Handle(
        CreateUser command,
        IDocumentSession session,
        IPasswordHasher hasher,
        IUserEmailReservationRepository repository,
        IIdentityService service,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        ValidateTeamAndRole(command);

        var contact = ContactDataFactory.CreateValueObjects(command);

        PasswordPolicyValidator.Validate(command.Password);

        var user = await service.GetUserAsync(command.CreatedBy, ct);
        var organizationId = OrganizationId.From(command.OrganizationId);

        await ValidateEmailReservation(repository, ct, organizationId, contact.Email);

        var userId = UserId.New();

        var passwordHash = hasher.Hash(command.Password);

        var organizationInfo = await service.GetOrganization(organizationId, ct);

        // Resolved before the user exists, so a bad team id is a 400 on this request rather than a
        // silent dead letter behind an already-issued 201.
        var team = await ResolveTeam(command, service, organizationId, ct);

        await repository.ReserveAsync(organizationId, contact.Email, userId, passwordHash);

        var @event = new UserCreated(
            userId.Value,
            organizationId.Value,
            command.Role,
            passwordHash,
            organizationInfo,
            user!,
            contact.ToContact(),
            clock.UtcNow,
            team
        );

        session.Events.StartStream<User>(userId.Value, @event);

        var messages = new OutgoingMessages();

        // Identity does not own team rosters, so seating the person is a request to Teams. The event
        // already carries the team, so the read model is right either way — this is what makes the
        // roster agree with it.
        if (team != null)
        {
            messages.Add(
                new AssignUserToTeam(
                    team.Id,
                    organizationId.Value,
                    userId.Value,
                    team.Role,
                    command.CreatedBy
                )
            );
        }

        return (@event, messages);
    }

    private static void ValidateTeamAndRole(CreateUser command)
    {
        if (command.TeamId.HasValue != command.TeamRole.HasValue)
            throw new ValidationException(TeamAndRoleTogetherMessage);
    }

    private static async Task<TeamInfo?> ResolveTeam(
        CreateUser command,
        IIdentityService service,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        if (command.TeamId is not { } teamId)
            return null;

        var team = await service.GetTeamAsync(teamId, organizationId, ct);

        return new TeamInfo(team.TeamId, team.Name, command.TeamRole!.Value);
    }

    private static async Task ValidateEmailReservation(
        IUserEmailReservationRepository repository,
        CancellationToken ct,
        OrganizationId organizationId,
        Email email
    )
    {
        if (await repository.ExistAsync(organizationId, email, ct))
            throw new BusinessRuleException(UserWithEmailMessage);
    }
}
