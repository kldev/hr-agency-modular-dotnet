using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.Employment;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Agency.Application.Employment.Start;

/// <summary>
/// Records what somebody is to this company. Not an aggregate handler: this is the first event of
/// the person's stream, so there is nothing to load yet.
/// </summary>
public static class StartAgencyEmploymentHandler
{
    public const string AlreadyEmployedMessage =
        "This person already has an employment record. Change its terms instead of adding a second one.";

    public const string WeeklyHoursRangeMessage = "Weekly hours must be between 0 and 168.";

    public static async Task<AgencyEmploymentStarted> Handle(
        StartAgencyEmployment command,
        IAgencyService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        if (command.WeeklyHours is { } hours and (< 0 or > 168))
            throw new ValidationException(WeeklyHoursRangeMessage);

        var user = await service.GetOrganizationMemberAsync(
            OrganizationIdOf(command),
            command.UserId,
            ct
        );

        var streamId = AgencyStreamId.ForEmployment(command.OrganizationId, command.UserId);

        var existing = await session.Events.AggregateStreamAsync<AgencyEmployment>(
            streamId,
            token: ct
        );

        // The stream id already makes a second record impossible to store; this turns the collision
        // into a sentence instead of a "stream already exists" from the database.
        if (existing is not null)
            throw new BusinessRuleException(AlreadyEmployedMessage);

        var startedBy = await service.GetUserAsync(command.StartedBy, ct);

        var @event = new AgencyEmploymentStarted(
            command.OrganizationId,
            command.UserId,
            user,
            command.ContractType,
            command.StartsOn,
            command.WeeklyHours,
            startedBy,
            clock.UtcNow
        );

        session.Events.StartStream<AgencyEmployment>(streamId, @event);

        return @event;
    }

    private static SharedKernel.Tenant.OrganizationId OrganizationIdOf(
        StartAgencyEmployment command
    ) => SharedKernel.Tenant.OrganizationId.From(command.OrganizationId);
}
