using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Interviews.ChangeFormat;

public sealed record ChangeInterviewFormat(
    Guid InterviewId,
    Guid OrganizationId,
    string Note,
    InterviewFormat Format,
    Guid ModifiedBy) : IUpdateCommand;