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
            .WithQuery(organizationId, query)
            .OrderByDescending(z=>z.ScheduleAt)
            .ToSlice(query, ct);
    }

    public async Task<InterviewProjection?> Get(Guid organizationId, Guid interviewId, CancellationToken ct)
    {
        return await session.Query<InterviewProjection>()
            .WithOrganizationId(organizationId)
            .WithInterviewId(interviewId).SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<InterviewProjection>> GetRange(Guid organizationId, InterviewsQuery query,
        CancellationToken ct)
    {
        return await session.Query<InterviewProjection>()
            .WithQuery(organizationId, query)
            .OrderByDescending(z => z.ScheduleAt)
            .ToListAsync(ct);
    }
}