using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Projects.Application.Documents.Remove;

public sealed record RemoveProjectDocument(
    [property: Identity] Guid ProjectId,
    Guid OrganizationId,
    Guid DocumentId,
    Guid ModifiedBy
) : IUpdateCommand;
