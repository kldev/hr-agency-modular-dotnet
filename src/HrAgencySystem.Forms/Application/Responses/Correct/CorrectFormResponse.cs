using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Forms.Application.Responses.Correct;

/// <summary>
/// Amends a submitted response. The reason is required: a silent edit of a signed consent is exactly
/// what the revision history exists to rule out.
/// </summary>
public sealed record CorrectFormResponse(
    [property: Identity] Guid ResponseId,
    Guid OrganizationId,
    IReadOnlyList<FieldAnswer> Answers,
    string Reason,
    Guid ModifiedBy
) : IUpdateCommand;
