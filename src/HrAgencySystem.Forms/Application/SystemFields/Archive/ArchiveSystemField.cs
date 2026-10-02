using HrAgencySystem.Forms.Domain;
using HrAgencySystem.SharedKernel.Commands;
using JetBrains.Annotations;

namespace HrAgencySystem.Forms.Application.SystemFields.Archive;

public sealed record ArchiveSystemField(Guid OrganizationId, Guid SystemFieldId, Guid ModifiedBy)
    : IUpdateCommand
{
    [UsedImplicitly] // Wolverine's [AggregateHandler] loads the stream by this id.
    public Guid Id => FormsStreamId.ForCatalogue(OrganizationId);
}
