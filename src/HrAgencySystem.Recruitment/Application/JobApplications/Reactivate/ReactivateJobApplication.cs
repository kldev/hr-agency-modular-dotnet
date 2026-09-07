using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Reactivate;

public sealed record ReactivateJobApplication(  
    Guid JobApplicationId,
    Guid OrganizationId,
    string Note,
    Guid ModifiedBy): IUpdateCommand;