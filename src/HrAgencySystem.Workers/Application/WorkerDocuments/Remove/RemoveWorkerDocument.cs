using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Workers.Application.WorkerDocuments.Remove;

public sealed record RemoveWorkerDocument(
    [property: Identity] Guid WorkerId,
    Guid OrganizationId,
    Guid DocumentId,
    Guid ModifiedBy
) : IUpdateCommand;
