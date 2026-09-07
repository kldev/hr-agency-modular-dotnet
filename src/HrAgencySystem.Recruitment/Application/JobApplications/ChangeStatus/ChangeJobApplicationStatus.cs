using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplications.ChangeStatus;

public sealed record ChangeJobApplicationStatus(  
    Guid JobApplicationId,
    Guid OrganizationId,
    string Note,
    JobApplicationUpdateStatus Status,
    Guid? InterviewId,
    Guid ModifiedBy): IUpdateCommand;