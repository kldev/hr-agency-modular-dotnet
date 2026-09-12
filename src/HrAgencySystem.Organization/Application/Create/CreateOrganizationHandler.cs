using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Organization.Application.Create;

public static class CreateOrganizationHandler
{
    public const string SlugAlreadyExitsMessage = "An organization slug already exits";
    
    public static async Task<OrganizationCreated> Handle(
        CreateOrganization command,
        IDocumentSession session,
        IOrganizationSlugReservationRepository repository,
        IClock clock,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.NewId();
        var (name, slug) = OrganizationDataFactory.Create(command);

        if (await repository.Exists(slug, ct))
            throw new BusinessRuleException(SlugAlreadyExitsMessage);
        
        if (command.EmailDomains.Count == 0 || command.EmailDomains.All(z=>string.IsNullOrEmpty(z.Trim())))
            throw new BusinessRuleException("No email domains specified");

        
        await repository.Reserve(organizationId, slug);

        var @event = new OrganizationCreated(
            organizationId.Value,
            name.Value,
            slug.Value,
            [..command.EmailDomains.Where(z=>z.Trim().Length >0)],
            clock.UtcNow);

        session.Events.StartStream<Domain.Organization>(organizationId.Value, @event);
        
        return @event;
    }


}