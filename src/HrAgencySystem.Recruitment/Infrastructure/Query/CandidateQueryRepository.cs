using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

public sealed class CandidateQueryRepository(IQuerySession session) : ICandidateQueryRepository
{
    public async Task<SliceResponse<CandidateProjection>> GetCandidates(Guid organizationId, CandidateQuery query, CancellationToken ct)
    {
        return await session.Query<CandidateProjection>()
            .WithOrganizationId(organizationId)
            .WithCompanyId(query.CompanyId)
            .WithTags(query.Tags)
            .WithStatus(query.Status)
            .WithSources(query.Sources)
            .WithSearch(query.Search)
            .ToSlice(query, ct);
    }

    public async Task<CandidateProjection?> GetCandidate(Guid organizationId, Guid candidateId, CancellationToken ct)
    {
        return await session.Query<CandidateProjection>()
            .WithOrganizationId(organizationId)
            .WithCandidateId(candidateId)
            .SingleOrDefaultAsync(ct);
    }
}