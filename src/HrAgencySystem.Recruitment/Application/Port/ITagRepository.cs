using HrAgencySystem.Recruitment.Documents;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface ITagRepository
{
    Task<Tag> GetTag(Guid tagId, CancellationToken ct);
}