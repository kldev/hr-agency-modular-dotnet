using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Tasks.Application;
using HrAgencySystem.Tasks.Application.Complete;
using HrAgencySystem.Tasks.Application.Create;
using HrAgencySystem.Tasks.Application.Reopen;
using HrAgencySystem.Tasks.Application.Update;
using HrAgencySystem.Tasks.Contracts.IntegrationEvents;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Domain.ValueObjects;
using HrAgencySystem.Tasks.Services;
using Marten;
using NSubstitute;
using static HrAgencySystem.UnitTests.Tasks.TaskScenario;

namespace HrAgencySystem.UnitTests.Tasks;

public class TaskItemHandlerTests
{
    private readonly TaskScenario _scenario = new();

    [Fact]
    public async Task Create_WithoutAssignee_IsTheCreatorsOwnTask()
    {
        var session = Substitute.For<IDocumentSession>();

        var created = await CreateTaskItemHandler.Handle(
            Create(OpportunityId),
            _scenario.Service,
            session,
            _scenario.Clock,
            CancellationToken.None
        );

        Assert.Equal(UserId, created.Assignee.Id);
        Assert.Equal(CompanyId, created.Company.Id);
        Assert.Equal(new TaskOpportunity(OpportunityId, "Recruitment Q4"), created.Opportunity);
        Assert.Equal(Now, created.CreatedAt);
        session.Events.Received(1).StartStream<TaskItem>(created.TaskId, created);
    }

    [Fact]
    public async Task Create_ForAColleague_AssignsThem()
    {
        var created = await CreateTaskItemHandler.Handle(
            Create(assigneeId: ColleagueId),
            _scenario.Service,
            Substitute.For<IDocumentSession>(),
            _scenario.Clock,
            CancellationToken.None
        );

        Assert.Equal(Colleague, created.Assignee);
        Assert.Equal(User, created.CreatedBy);
        Assert.Null(created.Opportunity);
    }

    [Fact]
    public async Task Create_InvalidInput_ReportsEveryProblem()
    {
        var command = Create() with
        {
            Title = " ",
            Description = new string('x', 2001),
            DueAt = default,
            Priority = (TaskPriority)42,
        };

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateTaskItemHandler.Handle(
                command,
                _scenario.Service,
                Substitute.For<IDocumentSession>(),
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(
            [
                TaskTitle.RequiredMessage,
                TaskItemInputValidator.DescriptionTooLongMessage,
                TaskItemInputValidator.DueAtRequiredMessage,
                TaskItemInputValidator.UnknownPriorityMessage,
            ],
            error.Errors
        );
    }

    [Fact]
    public async Task Create_ForACompanyOfAnotherOrganization_IsRefusedWithoutSayingWhy()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateTaskItemHandler.Handle(
                Create() with { CompanyId = Guid.NewGuid() },
                _scenario.Service,
                Substitute.For<IDocumentSession>(),
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(ICompanySnapshotRepository.NotFoundMessage, error.Message);
    }

    [Fact]
    public async Task Create_WithAnUnknownOpportunity_IsRefused()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateTaskItemHandler.Handle(
                Create(Guid.NewGuid()),
                _scenario.Service,
                Substitute.For<IDocumentSession>(),
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(TasksService.OpportunityNotFoundMessage, error.Message);
    }

    [Fact]
    public async Task Create_WithAnotherCompanysOpportunity_IsRefused()
    {
        var foreignDeal = Guid.NewGuid();
        _scenario
            .Opportunities.GetOpportunityAsync(foreignDeal, TaskScenario.Organization, Arg.Any<CancellationToken>())
            .Returns(new OpportunitySnapshot(foreignDeal, OrganizationId, Guid.NewGuid(), "Other deal"));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateTaskItemHandler.Handle(
                Create(foreignDeal),
                _scenario.Service,
                Substitute.For<IDocumentSession>(),
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(TasksService.OpportunityOfAnotherCompanyMessage, error.Message);
    }

    [Fact]
    public async Task Create_ForSomebodyOutsideTheOrganization_IsRefused()
    {
        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateTaskItemHandler.Handle(
                Create(assigneeId: Guid.NewGuid()),
                _scenario.Service,
                Substitute.For<IDocumentSession>(),
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(IUserSnapshotRepository.NotFoundMessage, error.Message);
    }

    [Fact]
    public async Task Complete_WithAnOpportunity_TellsSales()
    {
        var task = Open();

        var (completed, events, messages) = await CompleteTaskItemHandler.Handle(
            new CompleteTaskItem(task.Id, OrganizationId, UserId),
            task,
            _scenario.Service,
            _scenario.Clock,
            CancellationToken.None
        );

        Assert.Equal(1, completed.Completion);
        Assert.Equal(Now, completed.CompletedAt);
        Assert.Single(events);

        var message = Assert.Single(messages.OfType<OpportunityTaskCompleted>());
        Assert.Equal(
            new OpportunityTaskCompleted(OrganizationId, task.Id, 1, OpportunityId, task.Title, UserId, Now),
            message
        );
    }

    [Fact]
    public async Task Complete_WithoutAnOpportunity_SendsNothing()
    {
        var task = Open(withOpportunity: false);

        var (_, _, messages) = await CompleteTaskItemHandler.Handle(
            new CompleteTaskItem(task.Id, OrganizationId, UserId),
            task,
            _scenario.Service,
            _scenario.Clock,
            CancellationToken.None
        );

        Assert.Empty(messages);
    }

    [Fact]
    public async Task Complete_AfterReopening_CountsTheNextCompletion()
    {
        var task = Done();
        task.Apply(new HrAgencySystem.Tasks.Events.TaskItemReopened(task.Id, OrganizationId, User, Now));

        var (completed, _, messages) = await CompleteTaskItemHandler.Handle(
            new CompleteTaskItem(task.Id, OrganizationId, UserId),
            task,
            _scenario.Service,
            _scenario.Clock,
            CancellationToken.None
        );

        Assert.Equal(2, completed.Completion);
        Assert.Equal(2, messages.OfType<OpportunityTaskCompleted>().Single().Completion);
    }

    [Fact]
    public async Task Complete_ADoneTask_IsRefused()
    {
        var task = Done();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CompleteTaskItemHandler.Handle(
                new CompleteTaskItem(task.Id, OrganizationId, UserId),
                task,
                _scenario.Service,
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(TaskItemStatusPolicy.AlreadyDoneMessage, error.Message);
    }

    [Fact]
    public async Task Complete_AnotherOrganizationsTask_IsNotFound()
    {
        var task = Open(organizationId: Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CompleteTaskItemHandler.Handle(
                new CompleteTaskItem(task.Id, OrganizationId, UserId),
                task,
                _scenario.Service,
                _scenario.Clock,
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Reopen_AnOpenTask_IsRefused()
    {
        var task = Open();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ReopenTaskItemHandler.Handle(
                new ReopenTaskItem(task.Id, OrganizationId, UserId),
                task,
                _scenario.Service,
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(TaskItemStatusPolicy.NotDoneMessage, error.Message);
    }

    [Fact]
    public async Task Reopen_ADoneTask_ReturnsItToOpen()
    {
        var task = Done();

        var (reopened, events) = await ReopenTaskItemHandler.Handle(
            new ReopenTaskItem(task.Id, OrganizationId, UserId),
            task,
            _scenario.Service,
            _scenario.Clock,
            CancellationToken.None
        );

        Assert.Equal(task.Id, reopened.TaskId);
        Assert.Single(events);
    }

    [Fact]
    public async Task Update_ADoneTask_IsRefused()
    {
        var task = Done();

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            UpdateTaskItemHandler.Handle(
                Update(task),
                task,
                _scenario.Service,
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(TaskItemStatusPolicy.DoneCannotChangeMessage, error.Message);
    }

    [Fact]
    public async Task Update_KeepingTheSameDeal_DoesNotLookItUpAgain()
    {
        var task = Open();

        var (updated, _) = await UpdateTaskItemHandler.Handle(
            Update(task) with { Title = "Call the HR manager again", AssigneeId = ColleagueId },
            task,
            _scenario.Service,
            _scenario.Clock,
            CancellationToken.None
        );

        Assert.Equal("Call the HR manager again", updated.Title);
        Assert.Equal(Colleague, updated.Assignee);
        Assert.Equal(task.Opportunity, updated.Opportunity);
        await _scenario
            .Opportunities.DidNotReceive()
            .GetOpportunityAsync(Arg.Any<Guid>(), Arg.Any<SharedKernel.Tenant.OrganizationId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ToAnotherCompanysDeal_IsRefused()
    {
        var task = Open();
        var foreignDeal = Guid.NewGuid();
        _scenario
            .Opportunities.GetOpportunityAsync(foreignDeal, TaskScenario.Organization, Arg.Any<CancellationToken>())
            .Returns(new OpportunitySnapshot(foreignDeal, OrganizationId, Guid.NewGuid(), "Other deal"));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            UpdateTaskItemHandler.Handle(
                Update(task) with { OpportunityId = foreignDeal },
                task,
                _scenario.Service,
                _scenario.Clock,
                CancellationToken.None
            )
        );

        Assert.Equal(TasksService.OpportunityOfAnotherCompanyMessage, error.Message);
    }

    private static UpdateTaskItem Update(TaskItem task) =>
        new(
            task.Id,
            OrganizationId,
            UserId,
            task.Opportunity?.Id,
            task.Title,
            task.Description,
            task.DueAt,
            task.Priority,
            task.Assignee.Id
        );
}
