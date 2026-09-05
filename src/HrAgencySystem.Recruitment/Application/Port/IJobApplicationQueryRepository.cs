using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Domain.JobApplication;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface IJobApplicationQueryRepository
{
    Task<SliceResponse<JobApplicationProjection>> GetJobApplications(Guid organizationId, JobApplicationQuery query,
        CancellationToken ct);

    Task<JobApplicationProjection?> GetJobApplication(Guid organizationId, Guid jobApplicationId, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record JobApplicationQuery(
    string Search, 
    Guid? CompanyId, 
    Guid[] Tags, 
    JobApplicationStatus[]? Status,
    CandidateSource[] Sources,
    int Page, 
    int PageSize) : IPagedQuery;