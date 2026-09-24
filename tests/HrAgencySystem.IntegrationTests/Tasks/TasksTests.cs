using System.Net;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Endpoints.Tasks.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Tasks.Application;
using HrAgencySystem.Tasks.Application.Port;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Domain.ValueObjects;
using HrAgencySystem.Tasks.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Tasks;

/// <summary>
/// The salesperson's task list end to end. The host runs on the real clock, so the expected
/// contents of each range are worked out with the same <see cref="TaskRange"/> the server uses,
/// from task dates placed relative to its bounds - the test means the same on a Monday and a
/// Sunday, at noon and just before midnight.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class TasksTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    private readonly TasksTestClient _tasks = new(env.CreateClient().AsOrganizationRoles(), outputHelper);

    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanTasks();
        await Cleaner.CleanSales();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task Each_range_holds_the_open_tasks_due_before_its_end_overdue_included()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var now = DateTimeOffset.UtcNow;
        var day = TaskRange.Resolve(TaskRangeKind.Day, TasksTestClient.TimeZone, now);
        var month = TaskRange.Resolve(TaskRangeKind.Month, TasksTestClient.TimeZone, now);

        var due = new Dictionary<string, DateTimeOffset>
        {
            ["overdue"] = day.From.AddDays(-3),
            ["today"] = day.From.AddHours(12),
            ["tomorrow"] = day.To.AddHours(12),
            ["next month"] = month.To.AddDays(3),
        };

        var ids = new Dictionary<Guid, DateTimeOffset>();
        foreach (var (title, dueAt) in due)
        {
            var created = await _tasks.CreateAsync(
                _organizationId,
                _userId,
                TasksTestClient.Request(companyId, dueAt, title)
            );
            ids[created.TaskId] = dueAt;
        }

        foreach (var range in Enum.GetValues<TaskRangeKind>())
        {
            var bounds = TaskRange.Resolve(range, TasksTestClient.TimeZone, DateTimeOffset.UtcNow);
            var expected = ids.Where(t => t.Value < bounds.To).Select(t => t.Key).ToHashSet();

            await Eventually.AssertAsync(async () =>
            {
                var board = await _tasks.BoardAsync(_organizationId, _userId, range);

                Assert.Equal(bounds, new TaskRange(board.From, board.To));
                Assert.Equal(expected, board.Active.Select(t => t.Id).ToHashSet());
                Assert.Empty(board.Completed);
            });
        }

        var week = await _tasks.BoardAsync(_organizationId, _userId, TaskRangeKind.Week);
        Assert.True(week.Active.Single(t => t.Title == "overdue").IsOverdue);
        Assert.Equal(week.Active.OrderBy(t => t.DueAt).Select(t => t.Id), week.Active.Select(t => t.Id));
    }

    [Fact]
    public async Task A_done_task_moves_to_completed_and_back_when_reopened()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var task = await _tasks.CreateAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(companyId, DateTimeOffset.UtcNow.AddHours(-1))
        );

        await _tasks.CompleteAsync(_organizationId, _userId, task.TaskId);

        await Eventually.AssertAsync(async () =>
        {
            var board = await _tasks.BoardAsync(_organizationId, _userId, TaskRangeKind.Day);

            Assert.Empty(board.Active);
            var done = Assert.Single(board.Completed);
            Assert.Equal(TaskItemStatus.Done, done.Status);
            Assert.NotNull(done.CompletedAt);
            Assert.False(done.IsOverdue);
        });

        await _tasks.ReopenAsync(_organizationId, _userId, task.TaskId);

        await Eventually.AssertAsync(async () =>
        {
            var board = await _tasks.BoardAsync(_organizationId, _userId, TaskRangeKind.Day);

            Assert.Equal(task.TaskId, Assert.Single(board.Active).Id);
            Assert.Empty(board.Completed);
        });
    }

    [Fact]
    public async Task Completing_a_done_task_again_is_refused()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var task = await _tasks.CreateAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(companyId, DateTimeOffset.UtcNow)
        );
        await _tasks.CompleteAsync(_organizationId, _userId, task.TaskId);

        var response = await _tasks.CompleteResponseAsync(_organizationId, _userId, task.TaskId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(TaskItemStatusPolicy.AlreadyDoneMessage, problem!.Detail);
    }

    [Fact]
    public async Task Completing_a_task_of_a_deal_logs_it_on_the_opportunity()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var opportunity = await OpportunityTestClient.Create(
            _organizationId,
            companyId,
            title: "Recruitment Q4",
            createdById: _userId
        );
        var task = await _tasks.CreateAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(
                companyId,
                DateTimeOffset.UtcNow,
                "Send recruitment proposal",
                opportunity.OpportunityId
            )
        );

        await _tasks.CompleteAsync(_organizationId, _userId, task.TaskId);

        await Eventually.AssertAsync(
            async () =>
            {
                var activities = await SalesActivityTestClient.GetSliceAsync(
                    _organizationId,
                    opportunityId: opportunity.OpportunityId
                );

                var logged = Assert.Single(activities.Content);
                Assert.Equal(SalesActivityType.Task, logged.ActivityType);
                Assert.Equal("Recruitment Q4", logged.OpportunityTitle);
                Assert.Contains("Send recruitment proposal", logged.Note);

                var row = await OpportunityTestClient.Get(_organizationId, opportunity.OpportunityId);
                Assert.Equal(SalesActivityType.Task, row.LastActivityType);
                Assert.Equal(logged.CreatedAt, row.LastActivityAt);
            },
            TimeSpan.FromSeconds(10)
        );
    }

    [Fact]
    public async Task The_board_is_the_callers_own_and_can_be_narrowed_to_one_company()
    {
        var acme = await ProjectClient.CreateCompanyAsync(_organizationId);
        var other = await ProjectClient.CreateCompanyAsync(_organizationId);
        var colleague = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var mine = await _tasks.CreateAsync(_organizationId, _userId, TasksTestClient.Request(acme, now));
        var mineElsewhere = await _tasks.CreateAsync(_organizationId, _userId, TasksTestClient.Request(other, now));
        var theirs = await _tasks.CreateAsync(_organizationId, colleague, TasksTestClient.Request(acme, now));
        var handedToMe = await _tasks.CreateAsync(
            _organizationId,
            colleague,
            TasksTestClient.Request(acme, now, assigneeId: _userId)
        );

        await Eventually.AssertAsync(async () =>
        {
            var all = await _tasks.BoardAsync(_organizationId, _userId, TaskRangeKind.Week);
            Assert.Equal(
                new HashSet<Guid> { mine.TaskId, mineElsewhere.TaskId, handedToMe.TaskId },
                all.Active.Select(t => t.Id).ToHashSet()
            );

            var acmeOnly = await _tasks.BoardAsync(_organizationId, _userId, TaskRangeKind.Week, acme);
            Assert.Equal(
                new HashSet<Guid> { mine.TaskId, handedToMe.TaskId },
                acmeOnly.Active.Select(t => t.Id).ToHashSet()
            );
            Assert.All(acmeOnly.Active, t => Assert.Equal(acme, t.Company.Id));
        });

        Assert.DoesNotContain(theirs.TaskId, (await _tasks.BoardAsync(_organizationId, _userId, TaskRangeKind.Week)).Active.Select(t => t.Id));
    }

    [Fact]
    public async Task Another_organization_cannot_read_or_complete_a_task()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var task = await _tasks.CreateAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(companyId, DateTimeOffset.UtcNow)
        );
        await Eventually.AssertAsync(async () =>
            Assert.NotNull(await _tasks.GetAsync(_organizationId, _userId, task.TaskId))
        );

        var intruder = Guid.NewGuid();

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _tasks.GetResponseAsync(intruder, _userId, task.TaskId)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _tasks.CompleteResponseAsync(intruder, _userId, task.TaskId)).StatusCode
        );
        Assert.Empty((await _tasks.BoardAsync(intruder, _userId, TaskRangeKind.Month)).Active);
    }

    [Fact]
    public async Task A_task_for_another_organizations_company_is_refused()
    {
        var foreignCompany = await ProjectClient.CreateCompanyAsync(Guid.NewGuid());

        var response = await _tasks.CreateResponseAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(foreignCompany, DateTimeOffset.UtcNow)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(ICompanySnapshotRepository.NotFoundMessage, problem!.Detail);
    }

    [Fact]
    public async Task A_task_on_another_organizations_opportunity_is_refused()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var otherOrganization = Guid.NewGuid();
        var foreignCompany = await ProjectClient.CreateCompanyAsync(otherOrganization);
        var foreignDeal = await OpportunityTestClient.Create(otherOrganization, foreignCompany);

        var response = await _tasks.CreateResponseAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(companyId, DateTimeOffset.UtcNow, opportunityId: foreignDeal.OpportunityId)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(TasksService.OpportunityNotFoundMessage, problem!.Detail);
    }

    [Fact]
    public async Task A_task_on_another_companys_opportunity_is_refused()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var otherCompany = await ProjectClient.CreateCompanyAsync(_organizationId);
        var deal = await OpportunityTestClient.Create(_organizationId, otherCompany);

        var response = await _tasks.CreateResponseAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(companyId, DateTimeOffset.UtcNow, opportunityId: deal.OpportunityId)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(TasksService.OpportunityOfAnotherCompanyMessage, problem!.Detail);
    }

    [Fact]
    public async Task An_invalid_task_reports_every_problem()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);

        var response = await _tasks.CreateResponseAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(companyId, default, title: "")
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadWithJson<BadRequestDetails>(OutputHelper);
        Assert.Contains(TaskTitle.RequiredMessage, problem!.ValidationErrors);
        Assert.Contains(TaskItemInputValidator.DueAtRequiredMessage, problem.ValidationErrors);
    }

    [Fact]
    public async Task An_open_task_can_be_changed_and_handed_over()
    {
        var companyId = await ProjectClient.CreateCompanyAsync(_organizationId);
        var colleague = Guid.NewGuid();
        var task = await _tasks.CreateAsync(
            _organizationId,
            _userId,
            TasksTestClient.Request(companyId, DateTimeOffset.UtcNow)
        );

        var response = await _tasks.UpdateResponseAsync(
            _organizationId,
            _userId,
            task.TaskId,
            new UpdateTaskRequest(
                null,
                "Follow up contract",
                "Ask for the signed copy.",
                DateTimeOffset.UtcNow.AddDays(1),
                TaskPriority.High,
                colleague
            )
        );
        response.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
        {
            var row = await _tasks.GetAsync(_organizationId, _userId, task.TaskId);

            Assert.NotNull(row);
            Assert.Equal("Follow up contract", row.Title);
            Assert.Equal(TaskPriority.High, row.Priority);
            Assert.Equal(colleague, row.AssigneeId);
        });

        Assert.Empty((await _tasks.BoardAsync(_organizationId, _userId, TaskRangeKind.Month)).Active);
        Assert.Single((await _tasks.BoardAsync(_organizationId, colleague, TaskRangeKind.Month)).Active);
    }
}
