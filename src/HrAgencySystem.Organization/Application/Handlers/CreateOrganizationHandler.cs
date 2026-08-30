using HrAgencySystem.Organization.Application.Commands;
using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using Microsoft.Extensions.Configuration;

namespace HrAgencySystem.Organization.Application.Handlers;

public static class CreateOrganizationHandler
{
    public const string SlugAlreadyExitsMessage = "An organization slug already exits";
    
    public static async Task<OrganizationCreated> Handle(
        CreateOrganization command,
        IDocumentSession session,
        IOrganizationSlugReservationRepository repository,
        IClock clock,
        IConfiguration configuration,
        CancellationToken ct)
    {

        var allowFixedId = configuration["AllowFixedId"] == "1"; 
        
        var organizationId = command.fixedId.HasValue && allowFixedId ? new OrganizationId(command.fixedId.Value)  : OrganizationId.NewId();
        var (name, slug) = CreateValueObjects(command);

        if (await repository.Exists(slug, ct))
            throw new BusinessRuleException(SlugAlreadyExitsMessage);

        await repository.Reserve(organizationId, slug);

        var @event = new OrganizationCreated(
            organizationId.Value,
            name.Value,
            slug.Value,
            clock.UtcNow);

        session.Events.StartStream<Domain.Organization>(organizationId.Value, @event);
        
        return @event;
    }

    private static OrganizationData CreateValueObjects(CreateOrganization command)
    {
        var errors = new List<string>();
        var (slug, errorSlug) = OrganizationSlug.TryCreate(command.Slug);
        var (name, errorName) = OrganizationName.TryCreate(command.Name);

        if (errorSlug != null) errors.Add(errorSlug);
        if (errorName != null) errors.Add(errorName);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new OrganizationData(name!, slug!);
    }

    private sealed record OrganizationData(
        OrganizationName Name,
        OrganizationSlug Slug);
}