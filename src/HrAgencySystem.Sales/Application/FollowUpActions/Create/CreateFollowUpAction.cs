using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Sales.Application.FollowUpActions.Create;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record CreateFollowUpAction(
    Guid OrganizationId,
    Guid OpportunityId,
    string Content,
    DateTimeOffset FollowDateTime,
    Guid CreatedBy
) : ICreateCommand;
