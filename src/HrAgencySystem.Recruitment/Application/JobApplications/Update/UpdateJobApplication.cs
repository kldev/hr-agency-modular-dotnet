using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.Tenant;
using JasperFx;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Update;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record UpdateJobApplication(
    [property: Identity] Guid ApplicationId,
    OrganizationId OrganizationId,
    string Phone,
    string FirstName,
    string LastName,
    Guid ModifiedBy) : IUpdateCommand;
