using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.ChangeRecruiter;

// ReSharper disable once UnusedType.Global
public static class ChangeJobPostRecruiterHandler
{
    [AggregateHandler]
    public static async Task<(
        JobPostRecruiterChanged,
        Wolverine.Marten.Events,
        OutgoingMessages
    )> Handle(
        ChangeJobPostRecruiter command,
        JobPost aggregate,
        IRecruitmentService service,
        IJobPostQueryRepository queryRepository,
        IClock clock,
        CancellationToken ct
    )
    {
        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);
        var recruiter = await service.GetUserAsync(command.RecruiterId, ct);

        ValidateOrganization(command, aggregate);

        var @event = new JobPostRecruiterChanged(
            command.JobPostId,
            recruiter,
            clock.UtcNow,
            modifiedBy
        );

        var messages = new OutgoingMessages();

        // Taking a post over yourself is not worth an email.
        if (recruiter.Id != modifiedBy.Id)
        {
            var post = await queryRepository.GetJobPostInfo(command.JobPostId, ct);

            messages.Add(
                new SendJobPostRecruiterChanged(
                    Guid.NewGuid(),
                    nameof(ChangeJobPostRecruiterHandler),
                    command.JobPostId,
                    post.JobTitle,
                    recruiter.Email,
                    recruiter.Fullname,
                    modifiedBy.Fullname
                )
            );
        }

        return (@event, [@event], messages);
    }

    private static void ValidateOrganization(ChangeJobPostRecruiter command, JobPost aggregate)
    {
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException("Invalid organization id");
    }
}
