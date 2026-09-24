using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Forms.Application.Responses.SaveDraft;

/// <summary>
/// The whole current set of answers of an unfinished response - the wizard saves it on every
/// "Next", so a long document can be left half way and picked up again.
/// </summary>
public sealed record SaveFormResponseDraft(
    [property: Identity] Guid ResponseId,
    Guid OrganizationId,
    IReadOnlyList<FieldAnswer> Answers,
    Guid ModifiedBy
) : IUpdateCommand;
