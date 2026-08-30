using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.Extensions.Logging;
using Wolverine.Marten;

namespace HrAgencySystem.Organization.Application.Handlers;

public static class UpdateOrganizationSlugHandler
{
    [AggregateHandler]
    public static async Task<(UpdatedAggregate, Wolverine.Marten.Events)> Handle(UpdateOrganizationSlug command,
        Domain.Organization aggregate,
        ILogger logger, IOrganizationSlugReservationRepository repository, CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Not found " + command.OrganizationId);

        logger.LogInformation($"Updating organization slug {command.Slug} from {aggregate.Slug.Value}");

        var (slug, error) = OrganizationSlug.TryCreate(command.Slug);
        if (error != null) throw new ValidationException(error);

        if (aggregate.Slug.Value.Equals(command.Slug))
            throw new BusinessRuleException("Cannot update organization with the same slug");

        if (await repository.Exists(slug!, ct))
            throw new BusinessRuleException(CreateOrganizationHandler.SlugAlreadyExitsMessage);

        var @event = new OrganizationSlugUpdated(slug!.Value, command.OrganizationId);

        return (new UpdatedAggregate(), [@event]);
    }
}