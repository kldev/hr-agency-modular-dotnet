using HrAgencySystem.Projects.Events;
using Marten.Events.Projections;

namespace HrAgencySystem.Projects.Projections;

/// <summary>
/// Positions live on the project's stream but are listed one by one, so the events are regrouped
/// by the position they are about. The same shape <c>WorkerProjector</c> uses to fold assignment
/// events onto the person they belong to.
/// </summary>
public sealed class ProjectPositionProjector
    : MultiStreamProjection<ProjectPositionProjection, Guid>
{
    public ProjectPositionProjector()
    {
        Identity<ProjectPositionOpened>(@event => @event.Position.PositionId);
        Identity<ProjectPositionUpdated>(@event => @event.Position.PositionId);
        Identity<ProjectPositionArchived>(@event => @event.PositionId);
        Identity<ProjectPositionRestored>(@event => @event.PositionId);
        Identity<ProjectPositionStaffed>(@event => @event.PositionId);
        Identity<ProjectPositionUnstaffed>(@event => @event.PositionId);
    }
}
