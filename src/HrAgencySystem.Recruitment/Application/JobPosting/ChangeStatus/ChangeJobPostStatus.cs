using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobPosting.ChangeStatus;

public sealed record ChangeJobPostStatus(Guid JobPostId,Guid OrganizationId, JobPostStatusApi Status, Guid ModifiedBy) : IUpdateCommand;
