using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Sales.Application.Opportunities.ChangeResponsible;

public sealed record ChangeResponsiblePerson(  
    Guid OpportunityId,
    Guid OrganizationId,  
    Guid ResponsibleId, 
    Guid ModifiedBy): IUpdateCommand;