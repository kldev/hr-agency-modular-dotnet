using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Forms.Application.FormDefinitions.Archive;

public sealed record ArchiveForm([property: Identity] Guid FormId, Guid OrganizationId, Guid ModifiedBy)
    : IUpdateCommand;
