using HrAgencySystem.Organization.Application.Create;
using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.Extensions.Logging;
using Wolverine.Marten;

namespace HrAgencySystem.Organization.Application.Update;

public static class UpdateOrganizationHandler
{
    [AggregateHandler]
    public static async Task<(OrganizationUpdated, Wolverine.Marten.Events)> Handle(
        UpdateOrganization command,
        Domain.Organization aggregate,
        ILogger logger,
        IOrganizationSlugReservationRepository repository,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Organization", command.OrganizationId);

        logger.LogUpdateOrganization(command.Name, command.Slug, command.EmailDomains.Count);
        
        var organizationId = aggregate.Id;
        var (name, slug) = OrganizationDataFactory.Create(command);
        if (command.EmailDomains.Count == 0 || command.EmailDomains.All(z=>string.IsNullOrEmpty(z.Trim())))
            throw new BusinessRuleException("No email domains specified");

        if (aggregate.Slug.Value != slug.Value)
        {
            if (await repository.Exists(slug, ct))
                throw new BusinessRuleException(CreateOrganizationHandler.SlugAlreadyExitsMessage);
            await repository.Reserve(organizationId, slug);
        }

        var @event = new OrganizationUpdated(command.OrganizationId,
            name.Value, 
            slug.Value, 
            [..command.EmailDomains.Where(z=>z.Trim().Length >0)],
            clock.UtcNow);

        return (@event, [@event]);
    }
}

internal static partial class OrganizationLogs
{

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Updating organization {name} {slug}, domains count {count}")]
    public static partial void LogUpdateOrganization(this ILogger logger, 
        string name, string slug, int count);
}