using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Hire;

public sealed record HireCandidate(  
    Guid JobApplicationId,
    Guid ModifiedBy): IUpdateCommand;