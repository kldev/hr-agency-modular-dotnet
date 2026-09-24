using System.Security.Cryptography;
using System.Text;
using HrAgencySystem.Sales.Application.Activities.Create;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Tasks.Contracts.IntegrationEvents;
using Marten;
using Wolverine.Attributes;

namespace HrAgencySystem.Sales.Integration;

/// <summary>
/// A task of an opportunity was done in the tasks module; the opportunity's history says so, as an
/// activity of type <see cref="SalesActivityType.Task"/> logged by whoever ticked it off.
/// <para>
/// The activity id is derived from the task and the completion number. The message arrives at
/// least once, and a repeat then finds its activity already there instead of writing a second one;
/// a task reopened and done again carries the next number and is a genuinely new entry.
/// </para>
/// </summary>
[WolverineHandler]
public static class OpportunityTaskCompletedHandler
{
    public const string NotePrefix = "Task done: ";

    private static readonly Guid ActivityNamespace = Guid.Parse("6c1f0b9e-54d2-4a7e-9f3b-2d8a61c4e075");

    public static async Task Handle(
        OpportunityTaskCompleted message,
        ISalesService service,
        IDocumentSession session,
        CancellationToken ct
    )
    {
        var activityId = ActivityIdOf(message.TaskId, message.Completion);

        if (await session.Events.FetchStreamStateAsync(activityId, ct) is not null)
            return;

        var user = await service.GetUserAsync(message.CompletedById, ct);
        var note = ShortNote.Create(Truncate(NotePrefix + message.Title, ShortNote.MaxLength), false);

        await CreateActivityHandler.Log(
            session,
            service,
            SalesActivityId.From(activityId),
            message.OrganizationId,
            message.OpportunityId,
            SalesActivityType.Task,
            note,
            user,
            message.CompletedAt,
            ct
        );
    }

    public static Guid ActivityIdOf(Guid taskId, int completion)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes($"{ActivityNamespace:N}:{taskId:N}:{completion}"), hash);

        return new Guid(hash[..16]);
    }

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : value[..length];
}
