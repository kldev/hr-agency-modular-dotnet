using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Reject;

public sealed record RejectCandidate( 
    Guid JobApplicationId,
    Guid ModifiedBy) : IUpdateCommand;