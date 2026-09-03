using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobPosting.Archive;

public sealed record ArchiveJobPost(Guid JobPostingId, Guid ModifiedBy) : IUpdateCommand;
