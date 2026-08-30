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
    public static async Task<OrganizationSlugUpdated?> Handle(UpdateOrganizationSlug command, Eventstre organization, 
        ILogger logger, IOrganizationSlugReservationRepository repository, CancellationToken ct)
    {
        logger.LogInformation($"Updating organization slug {command.Slug} from {organization.Slug.Value}");
        
        var (slug, error) = OrganizationSlug.TryCreate(command.Slug);
        if (error != null) throw new ValidationException(error);

        if (organization.Slug.Value.Equals(command.Slug)) return null;
        
        if (await repository.Exists(slug!, ct))
            throw new BusinessRuleException(CreateOrganizationHandler.SlugAlreadyExitsMessage);

        return new OrganizationSlugUpdated(slug!.Value, command.OrganizationId);
    }
}