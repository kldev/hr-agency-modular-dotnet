using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Documents;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

public sealed class TagRepository(IQuerySession session) : ITagRepository
{
    public async Task<Tag> GetTag(Guid tagId, CancellationToken ct)
    {
        return await session.Query<Tag>().Where(z => z.Id == tagId).FirstAsync(ct);
    }
}