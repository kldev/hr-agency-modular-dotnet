using HrAgencySystem.Recruitment.Domain.JobPostings;

namespace HrAgencySystem.Recruitment.Application.Suggestion;

public interface IJobPostSuggestionRepository
{
    Task<IReadOnlyList<JobPostSuggestion>> GetPostSuggestions(
        Guid organizationId,
        string search,
        JobPostStatus? status,
        CancellationToken ct
    );
}
