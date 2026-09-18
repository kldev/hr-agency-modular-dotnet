using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Sales.Application.FollowUpActions.Update;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record UpdateFollowUpAction(
    Guid FollowUpActionId,
    Guid OrganizationId,
    string Content,
    DateTimeOffset FollowDateTime,
    Guid ModifiedBy) : IUpdateCommand;
