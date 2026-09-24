using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Forms.Application.SystemFields.AddStandard;

/// <summary>
/// An explicit click in the catalogue rather than something planted into every new organization:
/// it needs no integration with <c>Organization</c>, and an agency that wants none of these is never
/// handed them. Codes already present - archived ones too - are skipped, so repeating it is harmless.
/// </summary>
public static class AddStandardSystemFieldsHandler
{
    public static async Task<StandardSystemFieldsAdded> Handle(
        AddStandardSystemFields command,
        IFormsService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var streamId = FormsStreamId.ForCatalogue(command.OrganizationId);
        var catalogue = await session.Events.AggregateStreamAsync<SystemFieldCatalogue>(streamId, token: ct);

        var missing = StandardSystemFields.All
            .Where(definition => catalogue?.HasCode(definition.Code) != true)
            .ToList();

        if (missing.Count == 0)
            return new StandardSystemFieldsAdded(0);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);
        var now = clock.UtcNow;

        var events = missing
            .Select(definition => (object)new SystemFieldDefined(
                command.OrganizationId,
                new SystemField(
                    Guid.CreateVersion7(),
                    definition.Code,
                    definition.Type,
                    definition.Label,
                    definition.Description,
                    definition.Rules,
                    [],
                    definition.Source,
                    false
                ),
                createdBy,
                now
            ))
            .ToArray();

        if (catalogue is null)
            session.Events.StartStream<SystemFieldCatalogue>(streamId, events);
        else
            await session.Events.AppendExclusive(streamId, events);

        return new StandardSystemFieldsAdded(missing.Count);
    }
}
