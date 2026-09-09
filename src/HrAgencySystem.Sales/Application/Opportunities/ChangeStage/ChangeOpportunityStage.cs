using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Sales.Application.Opportunities.ChangeStage;

public sealed record ChangeOpportunityStage(
    Guid SalesOpportunityId,
    Guid OrganizationId,
    OpportunityStage Stage,
    string LostReason,
    Guid ModifiedBy) :IUpdateCommand;