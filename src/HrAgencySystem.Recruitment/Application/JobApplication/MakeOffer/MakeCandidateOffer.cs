using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.MakeOffer;

public sealed record MakeCandidateOffer(
    Guid JobApplicationId,
    Guid ModifiedBy) : IUpdateCommand;
