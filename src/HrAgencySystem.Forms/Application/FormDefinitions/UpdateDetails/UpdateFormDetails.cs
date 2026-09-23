using HrAgencySystem.Forms.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Forms.Application.FormDefinitions.UpdateDetails;

/// <summary>What people read about a form. The code and the cardinality are not here: neither ever changes.</summary>
public sealed record UpdateFormDetails(
    [property: Identity] Guid FormId,
    Guid OrganizationId,
    string Name,
    string? Description,
    FormKind Kind,
    Guid ModifiedBy
) : IUpdateCommand;
