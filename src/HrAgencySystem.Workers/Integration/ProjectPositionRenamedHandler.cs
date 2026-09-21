using HrAgencySystem.Projects.Contracts.IntegrationEvents;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Events;
using Marten;
using Wolverine.Attributes;

namespace HrAgencySystem.Workers.Integration;

/// <summary>
/// One rename in a project becomes one event per posting held against that role. The fan-out is
/// done here rather than left to a reader because the frozen name lives on each assignment's own
/// stream, which is what makes the register readable without asking another module anything.
/// <para>
/// Every posting is touched, finished ones included: the list of who ever worked as what has to
/// call the role by the name the system now uses. What was already printed onto a document is a
/// file and stays as it was.
/// </para>
/// </summary>
[WolverineHandler]
public static class ProjectPositionRenamedHandler
{
    public static async Task Handle(
        ProjectPositionRenamed message,
        IAssignmentsQueryRepository assignments,
        IDocumentSession session,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(message.OrganizationId);

        var affected = await assignments.GetAssignmentsOnPosition(
            organizationId,
            message.PositionId,
            ct
        );

        foreach (var assignment in affected)
        {
            session.Events.Append(
                assignment.AssignmentId,
                new AssignmentPositionRenamed(
                    assignment.AssignmentId,
                    message.OrganizationId,
                    assignment.WorkerId,
                    message.PositionId,
                    message.Name,
                    message.ContractName
                )
            );
        }
    }
}
