using HrAgencySystem.Recruitment.Application.Suggestion;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Projections;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

public class JobPostSuggestionRepository(IQuerySession session): IJobPostSuggestionRepository
{
    public async Task<IReadOnlyList<JobPostSuggestion>> GetPostSuggestions(Guid organizationId, string search, JobPostStatus? status, CancellationToken ct)
    {
        return await session.Query<JobPostProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .WithStatus(status)
            .OrderByDescending(z=>z.CreatedAt)
            .Take(25)
            .Select(z => new JobPostSuggestion(z.Id, z.Title, z.Company.Name))
            .ToListAsync(ct);

    }
}