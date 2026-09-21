using HrAgencySystem.Projects.Events;
using HrAgencySystem.Workers.Contracts.IntegrationEvents;
using Marten;
using Wolverine.Attributes;

namespace HrAgencySystem.Projects.Integration;

[WolverineHandler]
public static class AssignmentPositionUnstaffedHandler
{
    public static void Handle(AssignmentPositionUnstaffed message, IDocumentSession session) =>
        session.Events.Append(
            message.ProjectId,
            new ProjectPositionUnstaffed(
                message.ProjectId,
                message.OrganizationId,
                message.PositionId,
                message.AssignmentId
            )
        );
}
