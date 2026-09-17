namespace HrAgencySystem.Company.Application.Suggestion;

public interface ICompanySuggestionRepository
{
    Task<IReadOnlyList<CompanySuggestion>>  GetCompanySuggestions(Guid organizationId, string search, string countryCode, CancellationToken ct);

    Task<CompanySuggestion?> GetCompanySuggestion(Guid organizationId, Guid companyId, CancellationToken ct);
}