using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Extensions;

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

        /// <summary>
        /// True keeps only those a workers' file was opened from, false only those without one,
        /// null leaves the list alone.
        /// </summary>
        internal IQueryable<JobApplicationProjection> WithRegisteredAsWorker(bool? registered)
        {
            return registered switch
            {
                true => query.Where(q => q.WorkerId != null),
                false => query.Where(q => q.WorkerId == null),
                null => query,
            };
        }

        internal IQueryable<JobApplicationProjection> WithJobPostId(Guid? jobPostId)
        {
            return jobPostId.IsInvalid() ? query : query.Where(q => q.JobPostId == jobPostId);
        }

        internal IQueryable<JobApplicationProjection> WithCompanyId(Guid? companyId)
        {
            return companyId.IsInvalid() ? query : query.Where(q => q.CompanyId == companyId);
        }

        internal IQueryable<JobApplicationProjection> WithStatus(
            IReadOnlyList<JobApplicationStatus> statuses
        )
        {
            return statuses.Count == 0 ? query : query.Where(q => statuses.Contains(q.Status));
        }

        internal IQueryable<JobApplicationProjection> WithSources(
            IReadOnlyList<CandidateSource> sources
        )
        {
            return sources.Count == 0 ? query : query.Where(q => sources.Contains(q.Source));
        }

        internal IQueryable<JobApplicationProjection> WithTags(IReadOnlyList<Guid> tags)
        {
            return tags.Count == 0
                ? query
                : tags.Aggregate(
                    query,
                    (current, tag) => current.Where(q => q.TagsIds.Contains(tag))
                );
        }

        internal IQueryable<JobApplicationProjection> WithSearch(string search)
        {
            return string.IsNullOrWhiteSpace(search)
                ? query
                : query.Where(q =>
                    q.ApplicantFullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || q.ApplicantEmail.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || q.JobPostTitle.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || q.Company.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                );
        }
    }
}
