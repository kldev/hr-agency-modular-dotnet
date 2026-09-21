using HrAgencySystem.Projects.Events;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using Marten;
using Wolverine.Attributes;

namespace HrAgencySystem.Projects.Integration;

/// <summary>The other half of <see cref="AssignmentPositionStaffedHandler"/>, locking for the same
/// reason: postings on one project end in bunches too.</summary>
[WolverineHandler]
public static class AssignmentPositionUnstaffedHandler
{
    public static Task Handle(AssignmentPositionUnstaffed message, IDocumentSession session) =>
        session.Events.AppendExclusive(
            message.ProjectId,
            new ProjectPositionUnstaffed(
                message.ProjectId,
                message.OrganizationId,
                message.PositionId,
                message.AssignmentId
            )
        );
}
