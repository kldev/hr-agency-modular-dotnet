using HrAgencySystem.Forms.Domain.Values;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Forms.Application.Responses.Submit;

public sealed record SubmitFormResponse(
    [property: Identity] Guid ResponseId,
    Guid OrganizationId,
    IReadOnlyList<FieldAnswer> Answers,
    Guid ModifiedBy
) : IUpdateCommand;
