using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Domain.Opportunity.ValueObjects;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Extensions;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Sales.Application.Opportunity.Create;

public static class CreateSalesOpportunityHandler
{
    public static async Task<SalesOpportunityCreated> Handle(
        CreateSalesOpportunity command,
        ISalesService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var (title, description) = CreateValueObjects(command);
        
        var organizationId = OrganizationId.From(command.OrganizationId);
        await service.ValidateOrganization(command.OrganizationId, ct);
        
        var user = await service.GetUserAsync(command.CreatedBy, ct);
        var owner = await GetOwner(service, command.OwnerId, user, ct);
        var company = await service.GetCompanyAsync(command.CompanyId, ct);

        var opportunityId = SalesOpportunityId.New();
        var @event = new SalesOpportunityCreated(
            opportunityId.Value,
            organizationId.Value,
            company,
            title.Value,
            description.Value,
            SalesOpportunityStage.New,
            command.ExpectedValue,
            command.Currency,
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

    private static (OpportunityTitle title, LongText description) CreateValueObjects(CreateSalesOpportunity command)
    {
        var (title, titleError) = OpportunityTitle.TryCreate(command.Title);
        var (description, descriptionError) = LongText.TryCreate(command.Description);

        var errors = new List<string>();
        if (titleError != null) errors.Add(titleError);
        if (descriptionError != null) errors.Add(descriptionError);

        return errors.Count > 0 ? throw new ValidationException(errors) : (title!, description!);
    }
}