using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Forms.Application.SystemFields.Update;

/// <summary>
/// Everything about a system field that may change. Code and type are not here: changing either
/// would change what every value already given for the field means (plan 028 §3.4).
/// </summary>
public sealed record UpdateSystemField(
    Guid OrganizationId,
    Guid SystemFieldId,
    string Label,
    string? Description,
    FieldRules? Rules,
    IReadOnlyList<ChoiceOption>? Options,
    SystemFieldSource Source,
    Guid ModifiedBy
) : IUpdateCommand
{
    /// <summary>The stream this command loads: the catalogue's, derived from the organization.</summary>
    public Guid Id => FormsStreamId.ForCatalogue(OrganizationId);
}
