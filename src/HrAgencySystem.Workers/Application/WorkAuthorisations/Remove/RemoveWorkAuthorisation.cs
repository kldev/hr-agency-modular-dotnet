using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Workers.Application.WorkAuthorisations.Remove;

public sealed record RemoveWorkAuthorisation(
    [property: Identity] Guid WorkerId,
    Guid OrganizationId,
    Guid AuthorisationId,
    Guid ModifiedBy
) : IUpdateCommand;
