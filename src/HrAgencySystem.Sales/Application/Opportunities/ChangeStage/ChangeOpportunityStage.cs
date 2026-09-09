using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Sales.Application.Opportunities.ChangeStage;

public sealed record ChangeOpportunityStage(
    [property: Identity] Guid OpportunityId,
    Guid OrganizationId,
    OpportunityStage Stage,
    string LostReason,
    Guid ModifiedBy) :IUpdateCommand;