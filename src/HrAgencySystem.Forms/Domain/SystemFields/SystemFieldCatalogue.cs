using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Forms.Domain.SystemFields;

/// <summary>
/// The organization's system fields, whole, as one aggregate per organization - the stream id is
/// derived from the organization (<see cref="FormsStreamId.ForCatalogue"/>).
/// <para>
/// Whole for the reason the org chart is whole: "a code is used once" is a fact about the set.
/// Held as one list it is a check in memory inside the transaction that adds the field, instead of
/// a reservation document per code. Affordable because a catalogue is tens of fields.
/// </para>
/// </summary>
public sealed class SystemFieldCatalogue : IOrganizationDomain
{
    private List<SystemField>? _fields = [];

    private SystemFieldCatalogue() { }

    public static SystemFieldCatalogue Empty() => new();

    public OrganizationId OrganizationId { get; private set; }

    public IReadOnlyList<SystemField> Fields => _fields ?? [];

    public SystemField? FieldById(Guid systemFieldId) =>
        Fields.FirstOrDefault(field => field.SystemFieldId == systemFieldId);

    /// <summary>Archived fields keep their code: an archived <c>employee.pesel</c> still owns every value given for it.</summary>
    public bool HasCode(string code) => Fields.Any(field => field.Code == code);

    public void Apply(SystemFieldDefined @event)
    {
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        _fields = [.. Fields, @event.Field];
    }

    public void Apply(SystemFieldUpdated @event) =>
        Replace(
            @event.SystemFieldId,
            field => field with
            {
                Label = @event.Label,
                Description = @event.Description,
                Rules = @event.Rules,
                Options = @event.Options,
                Source = @event.Source,
            }
        );

    public void Apply(SystemFieldArchived @event) =>
        Replace(@event.SystemFieldId, field => field with { IsArchived = true });

    private void Replace(Guid systemFieldId, Func<SystemField, SystemField> change) =>
        _fields = [.. Fields.Select(field => field.SystemFieldId == systemFieldId ? change(field) : field)];
}
