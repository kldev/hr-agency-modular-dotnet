using HrAgencySystem.Company.Application.Model;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Projections;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Query;

public sealed class CompanySuggestionRepository(IDocumentSession session) : ICompanySuggestionRepository
{
    public async Task<IReadOnlyList<CompanySuggestion>> GetCompanySuggestions(Guid organizationId, string search,
        string countryCode, CancellationToken ct)
    {
        return await session.Query<CompanyProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .WithCountryCode(countryCode)
            .OrderByDescending(z => z.CreatedBy)
            .Take(25)
            .Select(z => z.ToSuggestion()).ToListAsync(ct);
    }
}