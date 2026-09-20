using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Projects.Application.LegalEntity.Change;

public sealed record ChangeProjectLegalEntity(
    Guid ProjectId,
    Guid OrganizationId,
    Guid LegalEntityId,
    Guid ModifiedBy
) : IUpdateCommand;
