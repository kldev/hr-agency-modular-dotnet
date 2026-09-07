using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Tags.Add;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record TagApplication(Guid TagId, Guid JobApplicationId, Guid OrganizationId, Guid CreatedBy) : ICreateCommand;
