using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Projects.Application.Create;

public static class CreateProjectHandler
{
    public static async Task<ProjectCreated> Handle(
        CreateProject command,
        IProjectService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var (name, description, assignment) = ProjectDataFactory.Create(command);

        await service.ValidateOrganization(command.OrganizationId, ct);

        var company = await service.GetCompanyAsync(organizationId, command.CompanyId, ct);
        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        // A project may start on a company whose paperwork is not finished yet - it starts as a
        // draft precisely so the two can be filled in in any order. Going live is where the
        // complete profile becomes a condition.
        var team = command.TeamId is null
            ? null
            : await service.GetTeamAsync(organizationId, command.TeamId.Value, ct);

        var projectId = ProjectId.New();

        var @event = new ProjectCreated(
            projectId.Value,
            organizationId.Value,
            company,
            name.Value,
            description.Value,
            command.EngagementType,
            assignment,
            team?.TeamId,
            team?.Name,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<Project>(projectId.Value, @event);

        return @event;
    }
}
