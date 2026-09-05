using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Recruitment.Application.JobPosting.PostToChannel;

public sealed record PostToChannel(Guid JobPostId, Guid OrganizationId, PostingChannelType Channel, Guid ModifiedBy) : IUpdateCommand;