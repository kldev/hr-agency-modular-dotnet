using Marten;
namespace HrAgencySystem.SharedKernel.Web;

public static class SliceExtensions
{
    public static async Task<SliceResponse<T>> ToSlice<T>(
        this IQueryable<T> query,
        int page = 1,
        int pageSize = 10, CancellationToken ct = default) where T: notnull
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 500);

        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize + 1)
            .ToListAsync(ct);

        return new SliceResponse<T>(
            [.. items.Take(normalizedPageSize)],
            items.Count > normalizedPageSize);
    }

    public static async Task<SliceResponse<T>> ToSlice<T>(
        this IQueryable<T> query,
        IPagedQuery pageQuery, CancellationToken ct = default) where T : notnull
    {
        return await query.ToSlice(pageQuery.Page, pageQuery.PageSize, ct);
    }
}