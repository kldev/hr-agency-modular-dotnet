using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Tasks.Application.Create;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using HrAgencySystem.Tasks.Services;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace HrAgencySystem.UnitTests.Tasks;

/// <summary>
/// One organization with one company, one of its deals and two people. The ports are substitutes
/// behind the real <see cref="TasksService"/>, so the tenant rules are the production ones.
/// </summary>
internal sealed class TaskScenario
{
    public static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid CompanyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OpportunityId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid UserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ColleagueId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static readonly DateTimeOffset Now = new(2026, 9, 24, 8, 0, 0, TimeSpan.Zero);

    public static readonly OrganizationId Organization = SharedKernel.Tenant.OrganizationId.From(OrganizationId);

    public static readonly UserSnapshot User = new(UserId, "Bob", "Wells", "bob@test.io");
    public static readonly UserSnapshot Colleague = new(ColleagueId, "Anna", "Nowak", "anna@test.io");
    public static readonly CompanySnapshot Company = new(CompanyId, "ACME Sp. z o.o.", "5260001234");

    public IUserSnapshotRepository Users { get; } = Substitute.For<IUserSnapshotRepository>();
    public ICompanySnapshotRepository Companies { get; } = Substitute.For<ICompanySnapshotRepository>();
    public IOpportunitySnapshotRepository Opportunities { get; } =
        Substitute.For<IOpportunitySnapshotRepository>();
    public IOrganizationChecker Checker { get; } = Substitute.For<IOrganizationChecker>();

    public IClock Clock { get; } = new FixedClock(Now);

    public TaskScenario()
    {
        Checker.Exists(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        Users.GetUserAsync(Arg.Any<Guid>(), Arg.Any<OrganizationId>(), Arg.Any<CancellationToken>()).ReturnsNull();
        Users.GetUserAsync(UserId, Organization, Arg.Any<CancellationToken>()).Returns(User);
        Users.GetUserAsync(ColleagueId, Organization, Arg.Any<CancellationToken>()).Returns(Colleague);

        Companies
            .GetCompanyAsync(Arg.Any<Guid>(), Arg.Any<OrganizationId>(), Arg.Any<CancellationToken>())
            .ReturnsNull();
        Companies.GetCompanyAsync(CompanyId, Organization, Arg.Any<CancellationToken>()).Returns(Company);

        Opportunities
            .GetOpportunityAsync(Arg.Any<Guid>(), Arg.Any<OrganizationId>(), Arg.Any<CancellationToken>())
            .ReturnsNull();
        Opportunities
            .GetOpportunityAsync(OpportunityId, Organization, Arg.Any<CancellationToken>())
            .Returns(new OpportunitySnapshot(OpportunityId, OrganizationId, CompanyId, "Recruitment Q4"));
    }

    public ITasksService Service => new TasksService(Users, Companies, Opportunities, Checker);

    public static CreateTaskItem Create(Guid? opportunityId = null, Guid? assigneeId = null) =>
        new(
            OrganizationId,
            UserId,
            CompanyId,
            opportunityId,
            "Call HR Manager",
            "Ask about the Q4 headcount.",
            Now.AddHours(3),
            TaskPriority.High,
            assigneeId
        );

    /// <summary>A task as the aggregate would be rebuilt from its stream.</summary>
    public static TaskItem Open(bool withOpportunity = true, Guid? organizationId = null)
    {
        var task = TaskItem.Empty();

        task.Apply(
            new TaskItemCreated(
                Guid.NewGuid(),
                organizationId ?? OrganizationId,
                "Call HR Manager",
                null,
                Now.AddHours(3),
                TaskPriority.Medium,
                Company,
                withOpportunity ? new TaskOpportunity(OpportunityId, "Recruitment Q4") : null,
                User,
                User,
                Now.AddDays(-1)
            )
        );

        return task;
    }

    public static TaskItem Done(bool withOpportunity = true)
    {
        var task = Open(withOpportunity);
        task.Apply(new TaskItemCompleted(task.Id, OrganizationId, 1, User, Now));

        return task;
    }
}
