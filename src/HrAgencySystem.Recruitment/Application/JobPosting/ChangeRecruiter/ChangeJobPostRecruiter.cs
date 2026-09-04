using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobPosting.ChangeRecruiter;

public sealed record ChangeJobPostRecruiter(
    Guid JobPostId, 
    Guid OrganizationId, 
    Guid RecruiterId, 
    Guid ModifiedBy): IUpdateCommand;
