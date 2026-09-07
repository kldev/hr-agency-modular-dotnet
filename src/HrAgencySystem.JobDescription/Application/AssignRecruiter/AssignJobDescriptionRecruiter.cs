using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.JobDescription.Application.AssignRecruiter;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record AssignJobDescriptionRecruiter(Guid JobDescriptionId, Guid RecruiterId, Guid ModifiedBy, Guid OrganizationId) : IUpdateCommand;
