using HrAgencySystem.Api.Auth;
using HrAgencySystem.JobDescription.Application.Port;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Projections;
using Marten;

namespace HrAgencySystem.Api.Endpoints.JobDescription.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/job-description", Handler).WithSummary("Get job descriptions");
    }

    private static async Task<IResult> Handler(IJobDescriptionQueryRepository repository,
        AppUserAuthenticated user,
        string? search,
        Guid? companyId,
        Guid? recruiterId,
        JobDescriptionStatus[]? status,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new JobDescriptionQuery(search ?? "", companyId, recruiterId, status ?? [], page, pageSize);
        var result = await repository.GetJobDescriptions(user.OrganizationId, query, ct);
        
        return TypedResults.Ok(result);
    }
}