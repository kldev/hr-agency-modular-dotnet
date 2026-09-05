using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidate.ValueObjects;
using HrAgencySystem.Recruitment.Domain.JobApplication;
using HrAgencySystem.Recruitment.Events.JobApplication;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Create;

public static class CreateJobApplicationHandler
{
    public static async Task<JobApplicationCreated> Handle(CreateJobApplication command,
        ICandidateResolver resolver,
        IJobPostQueryRepository queryRepository,
        ICompanySnapshotRepository companySnapshot,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct)
    {
        var (email, firstName, lastName, phoneNumber) = GetValueObjects(command);
        var post = await queryRepository.GetJobPostInfo(command.JobPostId, ct);
        var candidateCommand =
            new CreateCandidate(post.OrganizationId,
                command.Email,
                command.Source,
                command.Phone, 
                command.FirstName, 
                command.LastName, 
                null, 
                post.CompanyId);
        var candidate = await resolver.FindOrCreate(candidateCommand, post, ct);

        var company = await GetCompany(companySnapshot, post.CompanyId, ct);
        
        var jobApplicationId = JobApplicationId.New();
        var @event = new JobApplicationCreated(
            jobApplicationId.Value,
            post.OrganizationId, 
            post.Id, 
            post.JobTitle,
            command.Source, 
            company,
            candidate,
            email.Value,
            phoneNumber.Value,
            firstName.Value,
            lastName.Value,
            clock.UtcNow);

        session.Events.StartStream<Domain.JobApplication.JobApplication>(jobApplicationId.Value, @event);

        return @event;
    }

    private static async Task<CompanySnapshot> GetCompany(ICompanySnapshotRepository companySnapshot, Guid companyId,
        CancellationToken ct)
    {
        var result = await companySnapshot.GetCompanyAsync(companyId, ct);
        return result != null ? result! : throw new NotFoundException(ICompanySnapshotRepository.NotFoundMessage);
    }

    private static (Email email, FirstName firstName, LastName lastName, CandidatePhoneNumber phoneNumber)
        GetValueObjects(CreateJobApplication command)
    {
        var (email, emailError) = Email.TryCreate(command.Email);
        var (firstName, firstNameError) = FirstName.TryCreate(command.FirstName ?? "", false);
        var (lastName, lastNameError) = LastName.TryCreate(command.LastName ?? "", false);
        var (phoneNumber, phoneNumberError) = CandidatePhoneNumber.TryCreate(command.Phone);
        return emailError != null ? throw new ValidationException(emailError) : (email!, firstName!, lastName!, phoneNumber!);
    }
    
}