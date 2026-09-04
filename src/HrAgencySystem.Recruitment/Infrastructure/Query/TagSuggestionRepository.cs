using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

public sealed class TagSuggestionRepository(IQuerySession session) : ITagSuggestionRepository
{
    public async Task<IReadOnlyList<Tag>> GetSuggestions(string search, TagCategory? category, CancellationToken ct)
    {
        return await session.Query<Tag>().WithName(search).WithCategory(category).Take(25).ToListAsync(ct);
    }
}