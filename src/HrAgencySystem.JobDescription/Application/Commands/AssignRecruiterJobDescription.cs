namespace HrAgencySystem.JobDescription.Application.Commands;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record AssignRecruiterJobDescription(Guid JobDescriptionId, Guid RecruiterId, Guid ModifiedBy, Guid OrganizationId);
