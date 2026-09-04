using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.JobApplication;
using HrAgencySystem.Recruitment.Events.JobApplication;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Create;

public static class CreateJobApplicationHandler
{
    public static async Task<JobApplicationCreated> Handle(CreateJobApplication command,
        ICandidateResolver resolver,
        IJobPostQueryRepository queryRepository,
        IClock clock,
        CancellationToken ct)
    {
        var post = await queryRepository.GetJobPostInfo(command.JobPostId, ct);
        var candidateCommand =
            new CreateCandidate(post.OrganizationId,
                command.Email,
                command.Source,
                command.PhoneNumber, 
                null, 
                null, 
                null, 
                post.CompanyId);
        var candidate = await resolver.FindOrCreate(candidateCommand, ct);

        var jobApplicationId = JobApplicationId.New();
        var @event = new JobApplicationCreated(
            jobApplicationId.Value,
            post.OrganizationId, 
            post.Id, 
            candidate.CandidateId, 
            command.Source, 
            clock.UtcNow, 
            candidate.Email.Value);

        return @event;
    }
}