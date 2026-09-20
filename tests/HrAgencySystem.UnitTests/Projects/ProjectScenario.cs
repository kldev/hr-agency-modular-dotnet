using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;
using NSubstitute;

namespace HrAgencySystem.UnitTests.Projects;

/// <summary>
/// Builds a project at whatever point in its life a test needs, by replaying the events that would
/// have put it there. Nothing reaches into the aggregate's state directly - a test that set up a
/// state the events cannot produce would be testing a situation the system cannot be in.
/// </summary>
internal static class ProjectScenario
{
    public static readonly Guid OrganizationId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );
    public static readonly Guid ProjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CompanyId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid TeamId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid UserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static UserSnapshot User { get; } =
        new(UserId, "Alice", "Wells", "alice-wells@hr-agency.com");

    public static PostalAddress Workplace { get; } =
        PostalAddress.Create("Rue de la Loi", "16", null, "1000", "Bruxelles", "BE");

    public static PostalAddress RegisteredAddress { get; } =
        PostalAddress.Create("Prosta", "51", null, "00-838", "Warszawa", "PL");

    public static CompanySnapshot CompleteCompany { get; } =
        new(
            CompanyId,
            "ACME Corporation",
            "PL1234567890",
            true,
            RegisteredAddress,
            "ACME Corporation sp. z o.o.",
            "PL1234567890"
        );

    public static CompanySnapshot IncompleteCompany { get; } =
        new(CompanyId, "ACME Corporation", "PL1234567890");

    public static ContactPerson Responsible { get; } =
        new("marie@acme.example.com", "Marie", "Dubois", "Operations lead", "+32 2 000 00 00");

    public static ContactPerson Signatory { get; } =
        new("paul@acme.example.com", "Paul", "Janssens", "CFO", "+32 2 000 00 01");

    public static DateOnly StartsOn { get; } = new(2026, 10, 1);

    public static IProjectService Service(CompanySnapshot? company = null)
    {
        var service = Substitute.For<IProjectService>();

        service.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(User);
        service
            .GetCompanyAsync(
                Arg.Any<OrganizationId>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(company ?? CompleteCompany);
        service
            .GetTeamAsync(Arg.Any<OrganizationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new TeamSnapshot(TeamId, "Tiggers"));

        return service;
    }

    public static Project Draft(
        EngagementType engagement = EngagementType.TemporaryAgencyWork,
        Guid? organizationId = null
    )
    {
        var project = Project.Empty();

        project.Apply(
            new ProjectCreated(
                ProjectId,
                organizationId ?? OrganizationId,
                CompleteCompany,
                "Delivery for ACME",
                "Two developers on site in Brussels.",
                engagement,
                new Assignment(Workplace, "BE", StartsOn, null),
                null,
                null,
                User,
                DateTimeOffset.UtcNow
            )
        );

        return project;
    }

    public static Project WithResponsible(this Project project)
    {
        project.Apply(
            new ProjectContactAssigned(
                ProjectId,
                project.OrganizationId.Value,
                ContactRole.Responsible,
                Responsible,
                null,
                null,
                User,
                DateTimeOffset.UtcNow
            )
        );

        return project;
    }

    public static Project WithSignedContract(this Project project)
    {
        project.Apply(
            new ProjectContractRecorded(
                ProjectId,
                project.OrganizationId.Value,
                new ProjectContract(
                    "UM/2026/17",
                    ContractStatus.Signed,
                    new DateOnly(2026, 9, 15),
                    StartsOn,
                    null,
                    Signatory,
                    new CompanyPartySnapshot(
                        "ACME Corporation sp. z o.o.",
                        "PL1234567890",
                        "PL1234567890",
                        RegisteredAddress
                    ),
                    null
                ),
                User,
                DateTimeOffset.UtcNow
            )
        );

        return project;
    }

    public static Project InStatus(this Project project, ProjectStatus status)
    {
        project.Apply(
            new ProjectStatusChanged(
                ProjectId,
                project.OrganizationId.Value,
                project.Status,
                status,
                "",
                User,
                DateTimeOffset.UtcNow
            )
        );

        return project;
    }

    public static Project Live(this Project project) =>
        project.WithResponsible().WithSignedContract().InStatus(ProjectStatus.Active);
}
