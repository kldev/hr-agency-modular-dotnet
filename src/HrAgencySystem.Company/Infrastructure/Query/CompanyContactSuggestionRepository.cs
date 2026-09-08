using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.Company.Documents;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Query;

public sealed class CompanyContactSuggestionRepository(IQuerySession session) : ICompanyContactSuggestionRepository
{
    public async Task<IReadOnlyList<CompanyContact>> GetSuggestionAsync(Guid organizationId, string search, CancellationToken ct)
    {
        return await session.Query<CompanyContact>()
            .WithOrganizationId(OrganizationId.From(organizationId))
            .WithSearch(search)
            .OrderByDescending(z=>z.CreatedAt)
            .ThenBy(z=>z.FirstName)
            .Take(25)
            .ToListAsync(ct);
    }
}