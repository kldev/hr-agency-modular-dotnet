using HrAgencySystem.Recruitment.Documents;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

internal static class TagExtensions
{
    internal static IQueryable<Tag> WithName(this IQueryable<Tag> query, string name)
    {
        return string.IsNullOrWhiteSpace(name) ? query : query.Where(t => t.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    internal static IQueryable<Tag> WithCategory(this IQueryable<Tag> query, TagCategory? category)
    {
        return category.HasValue ? query.Where(t => t.Category == category) : query;
    }
}