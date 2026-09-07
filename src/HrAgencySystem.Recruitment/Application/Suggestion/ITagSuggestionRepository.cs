using HrAgencySystem.Recruitment.Documents;

namespace HrAgencySystem.Recruitment.Application.Suggestion;

public interface ITagSuggestionRepository
{
    Task<IReadOnlyList<Tag>>  GetSuggestions(string search, TagCategory? category, CancellationToken ct);
}