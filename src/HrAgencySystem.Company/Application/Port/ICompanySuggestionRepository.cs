using HrAgencySystem.Company.Application.Model;

namespace HrAgencySystem.Company.Application.Port;

public interface ICompanySuggestionRepository
{
    Task<IReadOnlyList<CompanySuggestion>>  GetCompanySuggestions(Guid organizationId, string search, string countryCode, CancellationToken ct);
}