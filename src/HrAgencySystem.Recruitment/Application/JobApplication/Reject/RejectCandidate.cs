using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Reject;

public sealed record RejectCandidate( 
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid JobApplicationId,
    Guid ModifiedBy) : IUpdateCommand;