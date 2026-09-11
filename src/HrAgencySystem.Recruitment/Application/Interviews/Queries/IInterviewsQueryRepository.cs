using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Recruitment.Application.Interviews.Queries;

public interface IInterviewsQueryRepository
{
    Task<SliceResponse<InterviewProjection>> GetSlice(Guid organizationId, InterviewsQuery query, CancellationToken ct);
    Task<InterviewProjection?> Get(Guid organizationId, Guid interviewId, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record InterviewsQuery(
    Guid? JobApplicationId,
    Guid? CandidateId,
    Guid? CreatedByUserId,
    Guid? InterviewerId,
    InterviewStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string Search,
    int Page, 
    int PageSize) : IPagedQuery;
    