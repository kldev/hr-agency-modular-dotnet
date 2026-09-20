using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Emails.Set;

public static class SetProjectEmailRecipientsHandler
{
    public const string DuplicateEmailMessage =
        "The same address is listed twice for this purpose.";

    [AggregateHandler]
    public static async Task<(ProjectEmailRecipientsChanged, Wolverine.Marten.Events)> Handle(
        SetProjectEmailRecipients command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var emails = Validate(command.Emails);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectEmailRecipientsChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            command.Purpose,
            emails,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    /// <summary>
    /// The whole set is replaced, so the whole set is validated at once. An empty list is a valid
    /// answer: it means nobody is to be mailed for this purpose.
    /// </summary>
    private static IReadOnlyList<string> Validate(IReadOnlyList<string>? input)
    {
        var errors = new List<string>();
        var emails = new List<string>();

        foreach (var candidate in input ?? [])
        {
            var (email, error) = Email.TryCreate(candidate);

            if (error is not null)
            {
                errors.Add(error);
                continue;
            }

            if (emails.Contains(email!.Value, StringComparer.OrdinalIgnoreCase))
                errors.Add(DuplicateEmailMessage);
            else
                emails.Add(email.Value);
        }

        return errors.Count > 0 ? throw new ValidationException(errors) : emails;
    }
}
