using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.Interviews.ChangeInterviewer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangeInterviewer(
    Guid InterviewId,
    Guid OrganizationId,
    Guid InterviewerId,
    string? Note,
    Guid ModifiedBy
) : IUpdateCommand;
