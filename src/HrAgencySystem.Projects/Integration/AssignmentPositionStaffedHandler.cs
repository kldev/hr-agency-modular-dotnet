using HrAgencySystem.Projects.Events;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using Marten;
using Wolverine.Attributes;

namespace HrAgencySystem.Projects.Integration;

/// <summary>
/// Staffing is decided in <c>Workers</c> and counted here. The translation is deliberately thin:
/// the other module says an assignment took a seat, this module records it on the project's own
/// stream and lets its own projection do the counting.
/// </summary>
[WolverineHandler]
public static class AssignmentPositionStaffedHandler
{
    public static void Handle(AssignmentPositionStaffed message, IDocumentSession session) =>
        session.Events.Append(
            message.ProjectId,
            new ProjectPositionStaffed(
                message.ProjectId,
                message.OrganizationId,
                message.PositionId,
                message.AssignmentId
            )
        );
}
