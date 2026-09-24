using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Documents;
using HrAgencySystem.Sales.Domain.FollowUp;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.FollowUp;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Sales.Services;

public sealed class SalesService(
    IUserSnapshotRepository userSnapshotRepository,
    ICompanySnapshotRepository companySnapshotRepository,
    IOpportunitySnapshotRepository salesOpportunitySnapshotRepository,
    IQueryFollowUpAction followUpActionQuery,
    IDocumentSession session,
    IClock clock,
    IOrganizationChecker checker
) : ISalesService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userSnapshotRepository.GetUserAsync(userId, ct);
        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var company = await companySnapshotRepository.GetCompanyAsync(companyId, ct);
        return company ?? throw new NotFoundException("Company", companyId);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(organizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    public async Task<OpportunitySnapshot> GetOpportunityAsync(
        Guid organizationId,
        Guid opportunityId,
        CancellationToken ct
    )
    {
        var opportunity = await salesOpportunitySnapshotRepository.GetOpportunityAsync(
            opportunityId,
            OrganizationId.From(organizationId),
            ct
        );

        return opportunity ?? throw new NotFoundException("Sales opportunity", opportunityId);
    }

    public void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId)
    {
        if (aggregate == null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new OrganizationAccessDeniedException();
    }

    public async Task<OrganizationId> GetBySlugAsync(string slug, CancellationToken ct)
    {
        return await checker.GetOrganizationIdBySlug(slug, ct);
    }

    public async Task<FollowUpActionCreated> AppendFollowUpActionToStream(
        SalesOpportunityId opportunityId,
        OrganizationId organizationId,
        string content,
        DateTimeOffset followDateTime,
        UserSnapshot user,
        CancellationToken ct
    )
    {
        var text = CreateContent(content);

        var opportunity = await GetOpportunityAsync(organizationId.Value, opportunityId.Value, ct);
        var company = await GetCompanyAsync(opportunity.CompanyId, ct);

        var document = FollowUpAction.Create(
            FollowUpActionId.New(),
            opportunity.OpportunityId,
            organizationId.Value,
            text,
            followDateTime,
            clock.UtcNow,
            company,
            user
        );

        var @event = new FollowUpActionCreated(
            document.Id,
            document.OpportunityId,
            document.OrganizationId,
            document.Content,
            document.FollowDateTime,
            document.CreatedAt,
            user
        );

        session.Events.Append(document.OpportunityId, @event);
        session.Insert(document);

        await session.SaveChangesAsync(ct);

        return @event;
    }

    public async Task<FollowUpActionUpdated> AppendFollowUpActionUpdateToStream(
        FollowUpActionId followUpActionId,
        OrganizationId organizationId,
        string content,
        DateTimeOffset followDateTime,
        UserSnapshot user,
        CancellationToken ct
    )
    {
        var text = CreateContent(content);

        var current =
            await followUpActionQuery.GetByIdAsync(organizationId.Value, followUpActionId.Value, ct)
            ?? throw new NotFoundException("Follow up action", followUpActionId.Value);

        var document = current.Update(text, followDateTime);

        var @event = new FollowUpActionUpdated(
            document.Id,
            document.OpportunityId,
            document.OrganizationId,
            document.Content,
            document.FollowDateTime,
            clock.UtcNow,
            user
        );

        session.Events.Append(document.OpportunityId, @event);
        session.Store(document);

        await session.SaveChangesAsync(ct);

        return @event;
    }

    private static LongText CreateContent(string content)
    {
        var (text, error) = LongText.TryCreate(content, true, FollowUpAction.ContentFieldName);

        return error != null ? throw new ValidationException(error) : text!;
    }
}
