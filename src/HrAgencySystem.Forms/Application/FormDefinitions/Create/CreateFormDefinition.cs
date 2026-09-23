using HrAgencySystem.Forms.Domain;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Forms.Application.FormDefinitions.Create;

/// <summary>
/// Opens a form with an empty draft. <paramref name="Cardinality"/> left out takes the kind's usual
/// answer: a document is one per person, a survey as many as there are occasions.
/// </summary>
public sealed record CreateFormDefinition(
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    FormKind Kind,
    ResponseCardinality? Cardinality,
    string SubjectKind,
    Guid CreatedBy
) : ICreateCommand;
