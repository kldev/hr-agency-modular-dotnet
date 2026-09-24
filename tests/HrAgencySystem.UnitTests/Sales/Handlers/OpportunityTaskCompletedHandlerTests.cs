using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Integration;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Tasks.Contracts.IntegrationEvents;
using JasperFx.Events;
using Marten;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace HrAgencySystem.UnitTests.Sales.Handlers;

public class OpportunityTaskCompletedHandlerTests
{
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid OpportunityId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly UserSnapshot User = new(Guid.NewGuid(), "Bob", "Wells", "bob@test.io");
    private static readonly DateTimeOffset CompletedAt = new(2026, 9, 24, 10, 0, 0, TimeSpan.Zero);

    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly ISalesService _service = Substitute.For<ISalesService>();

    public OpportunityTaskCompletedHandlerTests()
    {
        _service.GetUserAsync(User.Id, Arg.Any<CancellationToken>()).Returns(User);
        _service
            .GetOpportunityAsync(OrganizationId, OpportunityId, Arg.Any<CancellationToken>())
            .Returns(new OpportunitySnapshot(OpportunityId, OrganizationId, CompanyId, "Recruitment Q4"));
        _service
            .GetCompanyAsync(CompanyId, Arg.Any<CancellationToken>())
            .Returns(new CompanySnapshot(CompanyId, "ACME Sp. z o.o.", "5260001234"));
        _session.Events.FetchStreamStateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();
    }

    [Fact]
    public async Task Handle_LogsATaskActivityByWhoeverDidIt()
    {
        var message = Message(completion: 1);

        await OpportunityTaskCompletedHandler.Handle(message, _service, _session, CancellationToken.None);

        var activityId = OpportunityTaskCompletedHandler.ActivityIdOf(message.TaskId, 1);

        var activity = Assert.Single(Written<ActivityCreated>(nameof(IEventStoreOperations.StartStream), activityId));
        Assert.Equal(SalesActivityType.Task, activity.ActivityType);
        Assert.Equal(User, activity.CreatedBy);
        Assert.Equal("Recruitment Q4", activity.OpportunityTitle);
        Assert.Equal(OpportunityTaskCompletedHandler.NotePrefix + "Send recruitment proposal", activity.Note);
        Assert.Equal(CompletedAt, activity.CreatedAt);

        var logged = Assert.Single(Written<OpportunityActivityLogged>(nameof(IEventStoreOperations.Append), OpportunityId));
        Assert.Equal(activityId, logged.ActivityId);
        Assert.Equal(CompletedAt, logged.LoggedAt);
    }

    [Fact]
    public async Task Handle_ARepeatedMessage_WritesNothing()
    {
        var message = Message(completion: 1);
        _session
            .Events.FetchStreamStateAsync(
                OpportunityTaskCompletedHandler.ActivityIdOf(message.TaskId, 1),
                Arg.Any<CancellationToken>()
            )
            .Returns(new StreamState());

        await OpportunityTaskCompletedHandler.Handle(message, _service, _session, CancellationToken.None);

        Assert.Empty(Written<ActivityCreated>(nameof(IEventStoreOperations.StartStream), null));
    }

    [Fact]
    public void ActivityIdOf_EachCompletionIsItsOwnEntry()
    {
        var taskId = Guid.NewGuid();

        Assert.Equal(
            OpportunityTaskCompletedHandler.ActivityIdOf(taskId, 1),
            OpportunityTaskCompletedHandler.ActivityIdOf(taskId, 1)
        );
        Assert.NotEqual(
            OpportunityTaskCompletedHandler.ActivityIdOf(taskId, 1),
            OpportunityTaskCompletedHandler.ActivityIdOf(taskId, 2)
        );
    }

    /// <summary>
    /// The events handed to one kind of stream call, whichever overload the code picked - an
    /// argument matcher would only ever see one of them.
    /// </summary>
    private IEnumerable<T> Written<T>(string method, Guid? streamId) =>
        _session
            .Events.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == method)
            .Select(call => call.GetArguments())
            .Where(arguments => streamId is null || Equals(arguments[0], streamId))
            .SelectMany(arguments => arguments[1] switch
            {
                IEnumerable<object> events => events,
                var single => [single!],
            })
            .OfType<T>();

    private static OpportunityTaskCompleted Message(int completion) =>
        new(OrganizationId, Guid.NewGuid(), completion, OpportunityId, "Send recruitment proposal", User.Id, CompletedAt);
}
