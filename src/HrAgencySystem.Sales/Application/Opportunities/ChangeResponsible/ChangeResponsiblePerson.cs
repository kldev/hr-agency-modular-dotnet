using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Sales.Application.Opportunities.ChangeResponsible;

public sealed record ChangeResponsiblePerson(
    [property: Identity] Guid OpportunityId,
    Guid OrganizationId,
    Guid ResponsibleId,
    Guid ModifiedBy) : IUpdateCommand;
