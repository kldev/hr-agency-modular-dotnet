using HrAgencySystem.Projects.Events;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using Marten;
using Wolverine.Attributes;

namespace HrAgencySystem.Projects.Integration;

/// <summary>
/// Staffing is decided in <c>Workers</c> and counted here. The translation is deliberately thin:
/// the other module says an assignment took a seat, this module records it on the project's own
/// stream and lets its own projection do the counting.
/// <para>
/// <c>AppendExclusive</c> rather than <c>Append</c>, and that is not a detail: a crew is planned
/// onto one project several people at a time, each in its own transaction. A plain append lets two
/// of them work out the same next stream version and one dies on the primary key - which is what
/// the seeder produced, five lost messages sitting in the dead letter queue and a role that
/// claimed nobody was on it. The lock is held for one small append, and staffing a role is not on
/// anybody's critical path.
/// </para>
/// </summary>
[WolverineHandler]
public static class AssignmentPositionStaffedHandler
{
    public static Task Handle(AssignmentPositionStaffed message, IDocumentSession session) =>
        session.Events.AppendExclusive(
            message.ProjectId,
            new ProjectPositionStaffed(
                message.ProjectId,
                message.OrganizationId,
                message.PositionId,
                message.AssignmentId
            )
        );
}
