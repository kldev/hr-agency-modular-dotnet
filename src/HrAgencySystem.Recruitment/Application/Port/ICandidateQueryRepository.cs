using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface ICandidateQueryRepository
{
    Task<SliceResponse<CandidateProjection>> GetCandidates(Guid organizationId, CandidateQuery query, CancellationToken ct);
    Task<CandidateProjection?> GetCandidate(Guid organizationId, Guid candidateId, CancellationToken ct);
}

public sealed record CandidateQuery(
    string Search, 
    Guid? CompanyId, 
    Guid[] Tags, 
    CandidateStatus? Status,
    CandidateSource[] Sources,
    int Page, 
    int PageSize) : IPagedQuery;