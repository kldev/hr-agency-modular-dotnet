using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;


internal static class JobApplicationProjectionExtensions
{
    extension(IQueryable<JobApplicationProjection> query)
    {
        internal IQueryable<JobApplicationProjection> WithJobApplicationId(Guid jobApplicationId)
        {
            return query.Where(q => q.Id == jobApplicationId);
        }

        internal IQueryable<JobApplicationProjection> WithOrganizationId(Guid organizationId)
        {
            return query.Where(q => q.OrgId == organizationId);
        }

        internal IQueryable<JobApplicationProjection> WithCompanyId(Guid? companyId)
        {
            if (!companyId.HasValue || companyId.Value == Guid.Empty) return query;
        
            return query.Where(q => q.CompanyId == companyId.Value);
        }

        internal IQueryable<JobApplicationProjection> WithStatus(IReadOnlyList<JobApplicationStatus> statuses)
        {
            return statuses.Count == 0 ? query : query.Where(q => statuses.Contains(q.Status));
        }

        internal IQueryable<JobApplicationProjection> WithSources(IReadOnlyList<CandidateSource> sources)
        {
            return sources.Count == 0 ? query : query.Where(q => sources.Contains(q.Source));
        }

        internal IQueryable<JobApplicationProjection> WithTags(IReadOnlyList<Guid> tags)
        {
            if (tags.Count == 0)
                return query;

            return tags.Aggregate(query, (current, tag) => current.Where(q => q.TagsIds.Contains(tag)));
        }

        internal IQueryable<JobApplicationProjection> WithSearch(string search)
        {
            return  string.IsNullOrWhiteSpace(search) ? query : query.Where(q => 
                q.ApplicantFullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || q.ApplicantEmail.Contains(search, StringComparison.OrdinalIgnoreCase )
                || q.JobPostTitle.Contains(search, StringComparison.OrdinalIgnoreCase )
                || q.Company.Name.Contains(search, StringComparison.OrdinalIgnoreCase )
            );
        }
    }
}