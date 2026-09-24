using HrAgencySystem.Forms.Domain;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Forms.Application.SystemFields.Archive;

public sealed record ArchiveSystemField(Guid OrganizationId, Guid SystemFieldId, Guid ModifiedBy)
    : IUpdateCommand
{
    public Guid Id => FormsStreamId.ForCatalogue(OrganizationId);
}
