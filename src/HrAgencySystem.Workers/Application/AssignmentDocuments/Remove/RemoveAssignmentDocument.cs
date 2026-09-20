using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Workers.Application.AssignmentDocuments.Remove;

public sealed record RemoveAssignmentDocument(
    [property: Identity] Guid AssignmentId,
    Guid OrganizationId,
    Guid DocumentId,
    Guid ModifiedBy
) : IUpdateCommand;
