using HrAgencySystem.Company.Documents;

namespace HrAgencySystem.Company.Application.Suggestion;

public interface ICompanyContactSuggestionRepository
{
    Task<IReadOnlyList<CompanyContact>> GetSuggestionAsync(Guid organizationId, string search, CancellationToken ct);
}