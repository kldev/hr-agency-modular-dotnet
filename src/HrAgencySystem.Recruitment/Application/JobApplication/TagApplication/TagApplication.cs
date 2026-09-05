using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.TagApplication;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record TagApplication(Guid TagId, Guid JobApplicationId, Guid OrganizationId, Guid CreatedBy) : ICreateCommand;
