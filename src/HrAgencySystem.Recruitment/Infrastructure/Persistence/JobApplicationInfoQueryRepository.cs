using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Infrastructure.Query;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public partial class JobApplicationInfoQueryRepository(IQuerySession session, ILogger<JobApplicationInfoQueryRepository> logger) : IJobApplicationInfoQueryRepository
{
    public async Task<JobApplicationInfo?> GetAsync(Guid jobApplicationId, OrganizationId organizationId, CancellationToken ct)
    {
        var result = await session.Query<JobApplicationProjection>()
            .WithOrganizationId(organizationId.Value)
            .WithJobApplicationId(jobApplicationId)
            .Select(z => new JobApplicationInfo(z.Id, z.OrgId, z.CandidateId, z.CompanyId))
            .FirstOrDefaultAsync(ct);

        if (result != null) return result;

        LogFindJobApplicationInfoById(jobApplicationId);
        var data = await session.Query<JobApplicationCreated>()
            .Where(z => z.OrganizationId == organizationId.Value && z.JobApplicationId == jobApplicationId)
            .Select(z =>
                new JobApplicationInfo(z.JobApplicationId, z.OrganizationId, z.CandidateInfo.CandidateId, z.Company.Id))
            .SingleOrDefaultAsync(ct);

        if (data == null) return null;
        LogJobApplicationFoundInEvents(jobApplicationId);

        return data;
    }

    [LoggerMessage(LogLevel.Information, "Find job application info by id {JobApplicationId}")]
    partial void LogFindJobApplicationInfoById(Guid jobApplicationId);

    [LoggerMessage(LogLevel.Information, "Job application {JobApplicationId} found in events")]
    partial void LogJobApplicationFoundInEvents(Guid jobApplicationId);
}