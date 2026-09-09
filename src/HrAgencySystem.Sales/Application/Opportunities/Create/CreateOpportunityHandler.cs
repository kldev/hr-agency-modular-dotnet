using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Extensions;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Sales.Application.Opportunities.Create;

public static class CreateOpportunityHandler
{
    public static async Task<OpportunityCreated> Handle(
        CreateOpportunity command,
        ISalesService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var (title, description) = OpportunityDataFactory.Create(command);
        
        var organizationId = OrganizationId.From(command.OrganizationId);
        await service.ValidateOrganization(command.OrganizationId, ct);
        
        var user = await service.GetUserAsync(command.CreatedBy, ct);
        var owner = await GetOwner(service, command.ResponsibleId, user, ct);
        var company = await service.GetCompanyAsync(command.CompanyId, ct);

        var opportunityId = SalesOpportunityId.New();
        var @event = new OpportunityCreated(
            opportunityId.Value,
            organizationId.Value,
            company,
            title.Value,
            description.Value,
            OpportunityStage.New,
            command.ExpectedValue,
            command.Currency,
            command.IsHotLead,
            command.ExpectedCloseDate,
            owner,
            clock.UtcNow,
            user
            );

        session.Events.StartStream<SalesOpportunity>(organizationId.Value, @event);

        return @event;
    }

    private static async Task<UserSnapshot> GetOwner(ISalesService service, Guid? ownerId, UserSnapshot defaultOwner,
        CancellationToken ct)
    {
        if (ownerId.IsInvalid() || ownerId!.Value == defaultOwner.Id) return defaultOwner;

        var owner = await service.GetUserAsync(ownerId.Value, ct);

        return owner;
    }
}