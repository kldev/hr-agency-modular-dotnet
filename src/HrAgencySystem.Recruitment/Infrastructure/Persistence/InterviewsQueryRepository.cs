using HrAgencySystem.Recruitment.Application.Interviews.Queries;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public class InterviewsQueryRepository(IQuerySession session) : IInterviewsQueryRepository
{
    public async Task<SliceResponse<InterviewProjection>> GetSlice(Guid organizationId, InterviewsQuery query, CancellationToken ct)
    {
        return await session.Query<InterviewProjection>()
            .WithOrganizationId(organizationId)
            .WithInterviewerId(query.InterviewerId)
            .WithCandidateId(query.CandidateId)
            .WithCreatedByUserId(query.CreatedByUserId)
            .WithJobApplicationId(query.JobApplicationId)
            .WithStatus(query.Status)
            .WithScheduleFrom(query.From)
            .WithScheduleTo(query.To)
            .OrderByDescending(z=>z.ScheduleAt)
            .ToSlice(query, ct);
    }

    public async Task<InterviewProjection?> Get(Guid organizationId, Guid interviewId, CancellationToken ct)
    {
        return await session.Query<InterviewProjection>()
            .WithOrganizationId(organizationId)
            .WithInterviewId(interviewId).SingleOrDefaultAsync(ct);
    }
}