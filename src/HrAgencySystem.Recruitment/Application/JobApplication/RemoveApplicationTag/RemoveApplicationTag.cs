using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.RemoveApplicationTag;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record RemoveApplicationTag(Guid JobApplicationId, Guid TagId, Guid OrganizationId, Guid ModifiedBy) : IUpdateCommand;