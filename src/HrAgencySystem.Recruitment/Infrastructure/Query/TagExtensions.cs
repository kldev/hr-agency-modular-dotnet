using HrAgencySystem.Recruitment.Documents;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

internal static class TagExtensions
{
    extension(IQueryable<Tag> query)
    {
        internal IQueryable<Tag> WithName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? query : query.Where(t => t.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        internal IQueryable<Tag> WithCategory(TagCategory? category)
        {
            return category.HasValue ? query.Where(t => t.Category == category) : query;
        }
    }
}