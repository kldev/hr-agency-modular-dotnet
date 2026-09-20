using HrAgencySystem.Projects.Application.Contacts.Assign;
using HrAgencySystem.Projects.Application.Contacts.Remove;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.UnitTests.Projects;

public class ProjectContactHandlerTests : BaseTest
{
    [Fact]
    public async Task Assign_PutsThePersonInTheRole()
    {
        var project = ProjectScenario.Draft();

        var (result, _) = await Assign(project, ContactRole.Responsible, ProjectScenario.Responsible);

        Assert.Equal(ContactRole.Responsible, result.Role);
        Assert.Equal("Marie", result.Person.FirstName);
        Assert.Null(result.PreviousPerson);
    }

    [Fact]
    public async Task Assign_OverAnExistingRole_CarriesWhoWasThereBefore()
    {
        var project = ProjectScenario.Draft().WithResponsible();
        var replacement = new ContactPerson(
            "luc@acme.example.com",
            "Luc",
            "Peeters",
            "Delivery manager",
            "+32 2 000 00 02"
        );

        var (result, _) = await Assign(project, ContactRole.Responsible, replacement);

        Assert.Equal("Luc", result.Person.FirstName);
        Assert.Equal("Marie", result.PreviousPerson!.FirstName);
    }

    [Fact]
    public async Task Assign_ReplacesRatherThanAppends()
    {
        var project = ProjectScenario.Draft().WithResponsible();
        var replacement = ProjectScenario.Signatory;

        var (result, events) = await Assign(project, ContactRole.Responsible, replacement);
        project.Apply(result);

        Assert.Single(events);
        Assert.Single(project.Contacts);
        Assert.Equal("Paul", project.ContactInRole(ContactRole.Responsible)!.Person.FirstName);
    }

    [Fact]
    public async Task Assign_LeavesOtherRolesAlone()
    {
        // The signatory is who signed; the responsible contact is who answers today. Changing one
        // must never quietly change the other.
        var project = ProjectScenario.Draft().WithResponsible();

        var (signatory, _) = await Assign(
            project,
            ContactRole.ContractSignatory,
            ProjectScenario.Signatory
        );
        project.Apply(signatory);

        Assert.Equal(2, project.Contacts.Count);
        Assert.Equal("Marie", project.ContactInRole(ContactRole.Responsible)!.Person.FirstName);
        Assert.Equal("Paul", project.ContactInRole(ContactRole.ContractSignatory)!.Person.FirstName);
    }

    [Fact]
    public async Task Assign_WithABadContact_ThrowsValidation()
    {
        var project = ProjectScenario.Draft();
        var broken = new ContactPerson("not-an-email", "", "Dubois", "Lead", "+32 2 000 00 00");

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Assign(project, ContactRole.Responsible, broken)
        );

        Assert.Contains(Email.InvalidEmail, error.Errors);
        Assert.Contains(FirstName.RequiredMessage, error.Errors);
    }

    [Fact]
    public async Task Remove_TakesTheRoleOff()
    {
        var project = ProjectScenario.Draft().WithResponsible();

        var (result, _) = await Remove(project, ContactRole.Responsible);
        project.Apply(result);

        Assert.Empty(project.Contacts);
    }

    [Fact]
    public async Task Remove_AnUnassignedRole_Refuses()
    {
        var project = ProjectScenario.Draft();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Remove(project, ContactRole.Invoicing)
        );

        Assert.Equal(RemoveProjectContactHandler.RoleNotAssignedMessage, error.Message);
    }

    [Theory]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Suspended)]
    public async Task Remove_TheResponsibleContactOfALiveProject_Refuses(ProjectStatus status)
    {
        // The condition that had to hold to go live has to keep holding while it is live.
        var project = ProjectScenario.Draft().Live();
        if (status is ProjectStatus.Suspended)
            project.InStatus(ProjectStatus.Suspended);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Remove(project, ContactRole.Responsible)
        );

        Assert.Equal(
            RemoveProjectContactHandler.ResponsibleRequiredWhileLiveMessage,
            error.Message
        );
    }

    [Fact]
    public async Task Remove_TheResponsibleContactOfADraft_IsAllowed()
    {
        var project = ProjectScenario.Draft().WithResponsible();

        var (result, _) = await Remove(project, ContactRole.Responsible);

        Assert.Equal(ContactRole.Responsible, result.Role);
    }

    private static Task<(
        HrAgencySystem.Projects.Events.ProjectContactAssigned,
        Wolverine.Marten.Events
    )> Assign(Project project, ContactRole role, ContactPerson person) =>
        AssignProjectContactHandler.Handle(
            new AssignProjectContact(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                role,
                person,
                null,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );

    private static Task<(
        HrAgencySystem.Projects.Events.ProjectContactRemoved,
        Wolverine.Marten.Events
    )> Remove(Project project, ContactRole role) =>
        RemoveProjectContactHandler.Handle(
            new RemoveProjectContact(
                ProjectScenario.ProjectId,
                ProjectScenario.OrganizationId,
                role,
                ProjectScenario.UserId
            ),
            project,
            ProjectScenario.Service(),
            new FixedClock(DateTimeOffset.UtcNow),
            CancellationToken.None
        );
}
