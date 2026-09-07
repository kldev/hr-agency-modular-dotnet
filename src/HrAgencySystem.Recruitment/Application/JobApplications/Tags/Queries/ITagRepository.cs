using HrAgencySystem.Recruitment.Documents;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Tags.Queries;

public interface ITagRepository
{
    Task<Tag> GetTag(Guid tagId, CancellationToken ct);
}