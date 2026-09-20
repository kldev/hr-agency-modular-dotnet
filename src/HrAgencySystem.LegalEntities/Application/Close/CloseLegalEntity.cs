using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.LegalEntities.Application.Close;

public sealed record CloseLegalEntity(
    Guid LegalEntityId,
    Guid OrganizationId,
    DateOnly ActiveTo,
    Guid ModifiedBy
) : IUpdateCommand;
