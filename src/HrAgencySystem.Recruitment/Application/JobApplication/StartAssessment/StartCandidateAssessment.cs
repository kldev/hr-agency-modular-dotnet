using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.StartAssessment;

public sealed record StartCandidateAssessment(
    Guid JobApplicationId,
    Guid ModifiedBy) : IUpdateCommand;