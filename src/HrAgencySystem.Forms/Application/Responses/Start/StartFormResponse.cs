using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Forms.Application.Responses.Start;

/// <summary>
/// Opens a response to the form's current version for one person. For a form that takes one response
/// per person, a second start answers with the response that already exists instead of failing - the
/// caller wanted to fill it in, and that is where it is.
/// </summary>
public sealed record StartFormResponse(
    Guid OrganizationId,
    Guid FormId,
    string SubjectKind,
    Guid SubjectId,
    Guid CreatedBy
) : ICreateCommand;
