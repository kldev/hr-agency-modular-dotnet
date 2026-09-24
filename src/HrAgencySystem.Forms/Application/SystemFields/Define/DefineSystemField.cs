using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Forms.Application.SystemFields.Define;

public sealed record DefineSystemField(
    Guid OrganizationId,
    string Code,
    FieldType Type,
    string Label,
    string? Description,
    FieldRules? Rules,
    IReadOnlyList<ChoiceOption>? Options,
    SystemFieldSource Source,
    Guid CreatedBy
) : ICreateCommand;
