using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Interviews.ChangeStatus;

public sealed record ChangeInterviewStatus(
    Guid InterviewId,
    Guid OrganizationId,
    string Note,
    InterviewStatus Status,
    Guid ModifiedBy) : IUpdateCommand;