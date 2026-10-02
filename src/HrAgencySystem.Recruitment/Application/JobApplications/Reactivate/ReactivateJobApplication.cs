using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Reactivate;

// ReSharper disable once ClassNeverInstantiated.Global - handled, but no endpoint sends it yet.
public sealed record ReactivateJobApplication(
    Guid JobApplicationId,
    Guid OrganizationId,
    string Note,
    Guid ModifiedBy
) : IUpdateCommand;
