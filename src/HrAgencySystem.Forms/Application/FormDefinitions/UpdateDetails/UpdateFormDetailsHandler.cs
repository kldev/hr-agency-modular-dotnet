using HrAgencySystem.Forms.Application.FormDefinitions.Create;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.ValueObjects;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.FormDefinitions.UpdateDetails;

public static class UpdateFormDetailsHandler
{
    public const string ArchivedMessage = "This form is archived. It can be read, not changed.";

    [AggregateHandler]
    public static async Task<(FormDetailsUpdated, Wolverine.Marten.Events)> Handle(
        UpdateFormDetails command,
        FormDefinition aggregate,
        IFormsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId, "Form", command.FormId);

        if (aggregate.IsArchived)
            throw new BusinessRuleException(ArchivedMessage);

        var errors = new List<string>();

        var (name, nameError) = FormName.TryCreate(command.Name);
        if (nameError is not null)
            errors.Add(nameError);

        var description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        if (description is { Length: > 2000 })
            errors.Add(CreateFormDefinitionHandler.DescriptionTooLongMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new FormDetailsUpdated(
            command.OrganizationId,
            command.FormId,
            name!.Value,
            description,
            command.Kind,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
