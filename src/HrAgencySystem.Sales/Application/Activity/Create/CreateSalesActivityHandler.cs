using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Sales.Application.Activity.Create;

public static class CreateSalesActivityHandler
{
    public static async Task<SalesActivityCreated> Handle(CreateSalesActivity command,
        IOrganizationChecker checker,
        IUserSnapshotRepository userSnapshotRepository,
        ICompanySnapshotRepository companySnapshotRepository,
        ISalesOpportunitySnapshotRepository opportunitySnapshotRepository,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        await ValidateOrganization(checker, command.OrganizationId, ct);
        
        var note = CreateValueObjects(command);

        var user = await GetUser(userSnapshotRepository, command.CreatedBy, ct);
        var opportunity = await GetOpportunity(opportunitySnapshotRepository, command.SalesOpportunityId,
            command.OrganizationId, ct);
        var company = await GetCompany(companySnapshotRepository, opportunity.CompanyId, ct);

        var activityId = SalesActivityId.New();
        var @event = new SalesActivityCreated(
            activityId.Value,
            organizationId.Value, 
            opportunity.OpportunityId, 
            command.ActivityType, 
            note.Value,
            clock.UtcNow, user, company);

        session.Events.StartStream<SalesActivity>(activityId.Value, @event);
        return @event;
    }

    private static ShortNote CreateValueObjects(CreateSalesActivity command)
    {
        return ShortNote.Create(command.Note, false);
    }
    
    private static async Task ValidateOrganization(IOrganizationChecker checker, Guid organizationId,
        CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(organizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }
    
    private static async Task<UserSnapshot> GetUser(IUserSnapshotRepository repository, Guid userId,
        CancellationToken ct)
    {
        var user = await repository.GetUserAsync(userId, ct);
        return user ?? throw new NotFoundException("User", userId);
    }
    
    private static async Task<OpportunitySnapshot> GetOpportunity(ISalesOpportunitySnapshotRepository repository, 
        Guid opportunityId, Guid organizationId,
        CancellationToken ct)
    {
        var company = await repository.GetSnapshot(opportunityId, organizationId,ct);
        return company ?? throw new NotFoundException("Sales opportunity", opportunityId);
    }
    
    private static async Task<CompanySnapshot> GetCompany(ICompanySnapshotRepository repository, Guid companyId,
        CancellationToken ct)
    {
        var company = await repository.GetCompanyAsync(companyId, ct);
        return company ?? throw new NotFoundException("Company", companyId);
    }
}