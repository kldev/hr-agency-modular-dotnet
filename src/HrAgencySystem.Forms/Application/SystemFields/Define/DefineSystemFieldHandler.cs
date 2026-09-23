using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Domain.ValueObjects;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Forms.Application.SystemFields.Define;

/// <summary>
/// Not an aggregate handler: the organization's first field is also the first event of its
/// catalogue's stream, so there may be nothing to load yet - the <c>CreateOrgUnitHandler</c> case.
/// </summary>
public static class DefineSystemFieldHandler
{
    public static async Task<SystemFieldDefined> Handle(
        DefineSystemField command,
        IFormsService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var streamId = FormsStreamId.ForCatalogue(command.OrganizationId);
        var catalogue = await session.Events.AggregateStreamAsync<SystemFieldCatalogue>(streamId, token: ct);

        var field = Prepare(command, catalogue);
        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var @event = new SystemFieldDefined(command.OrganizationId, field, createdBy, clock.UtcNow);

        if (catalogue is null)
            session.Events.StartStream<SystemFieldCatalogue>(streamId, @event);
        else
            await session.Events.AppendExclusive(streamId, @event);

        return @event;
    }

    internal static SystemField Prepare(DefineSystemField command, SystemFieldCatalogue? catalogue)
    {
        var errors = new List<string>();

        var (code, codeError) = FieldCode.TryCreate(command.Code);

        if (codeError is not null)
            errors.Add(codeError);
        else if (!code!.IsSystemNamespace)
            errors.Add(SystemFieldRules.CodeNotSystemMessage);
        else if (catalogue?.HasCode(code.Value) == true)
            errors.Add(SystemFieldRules.CodeTakenMessage);

        var label = (command.Label ?? "").Trim();
        var description = SystemFieldRules.Blank(command.Description);
        var (rules, options) = FieldRulesPolicy.Normalize(command.Type, command.Rules, command.Options);

        errors.AddRange(SystemFieldRules.Check(command.Type, label, description, rules, options, command.Source));

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new SystemField(
            Guid.CreateVersion7(),
            code!.Value,
            command.Type,
            label,
            description,
            rules,
            options,
            command.Source,
            false
        );
    }
}
