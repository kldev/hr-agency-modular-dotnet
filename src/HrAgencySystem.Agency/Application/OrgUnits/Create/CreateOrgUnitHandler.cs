using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.ValueObjects;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.Create;

/// <summary>
/// The one command that cannot be an aggregate handler: the first unit is also the first event of
/// the organization's stream, so there is nothing to load yet.
/// </summary>
public static class CreateOrgUnitHandler
{
    public const string RootAlreadyExistsMessage =
        "This organization already has a top unit. Every other unit hangs under something.";

    public const string UnknownParentMessage = "There is no such parent unit.";

    public const string ParentArchivedMessage =
        "That parent unit is archived, so nothing new goes under it.";

    public const string NameAlreadyUsedMessage =
        "Another unit under the same parent already has this name.";

    public static async Task<OrgUnitCreated> Handle(
        CreateOrgUnit command,
        IAgencyService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var (name, error) = OrgUnitName.TryCreate(command.Name);

        if (error is not null)
            throw new ValidationException(error);

        // The chart sits on its own stream, derived from the organization - see OrgStructureId.
        var streamId = OrgStructureId.For(command.OrganizationId);

        var structure = await session.Events.AggregateStreamAsync<OrgStructure>(
            streamId,
            token: ct
        );

        Validate(structure, command, name!);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var @event = new OrgUnitCreated(
            command.OrganizationId,
            Guid.CreateVersion7(),
            command.ParentId,
            name!.Value,
            command.Kind,
            createdBy,
            clock.UtcNow
        );

        if (structure is null)
            session.Events.StartStream<OrgStructure>(streamId, @event);
        else
            await session.Events.AppendExclusive(streamId, @event);

        return @event;
    }

    private static void Validate(OrgStructure? structure, CreateOrgUnit command, OrgUnitName name)
    {
        if (structure is null)
        {
            // Nothing exists yet, so this has to be the root - hanging the first box under a parent
            // that cannot exist is the one way to get a chart with no top.
            if (command.ParentId is not null)
                throw new BusinessRuleException(UnknownParentMessage);

            return;
        }

        if (command.ParentId is null)
        {
            if (structure.Root is not null)
                throw new BusinessRuleException(RootAlreadyExistsMessage);
        }
        else
        {
            var parent =
                structure.UnitById(command.ParentId.Value)
                ?? throw new BusinessRuleException(UnknownParentMessage);

            if (parent.IsArchived)
                throw new BusinessRuleException(ParentArchivedMessage);
        }

        if (structure.HasSiblingNamed(command.ParentId, name.Value))
            throw new BusinessRuleException(NameAlreadyUsedMessage);
    }
}
