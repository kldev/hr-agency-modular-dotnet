using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobApplication;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplication.Create;

public static class ApplyToJobApplicationHandler
{
    public static async Task<JobApplicationCreated> Handle(ApplyToJobApplication command,
        ICandidateResolver resolver,
        IJobPostQueryRepository queryRepository,
        ICompanySnapshotRepository companySnapshot,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct)
    {
        var (email, firstName, lastName, phoneNumber) = GetValueObjects(command);
        var post = await queryRepository.GetJobPostInfo(command.JobPostId, ct);

        if (post.Status != JobPostStatus.Published)
            throw new BusinessRuleException("Applications are only allowed for published job posts.");
        
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

        session.Events.StartStream<Domain.Applications.JobApplication>(jobApplicationId.Value, @event);

        return @event;
    }

    private static async Task<CompanySnapshot> GetCompany(ICompanySnapshotRepository companySnapshot, Guid companyId,
        CancellationToken ct)
    {
        var result = await companySnapshot.GetCompanyAsync(companyId, ct);
        return result != null ? result! : throw new NotFoundException(ICompanySnapshotRepository.NotFoundMessage);
    }

    private static (Email email, FirstName firstName, LastName lastName, CandidatePhoneNumber phoneNumber)
        GetValueObjects(ApplyToJobApplication command)
    {
        var (email, emailError) = Email.TryCreate(command.Email);
        var (firstName, firstNameError) = FirstName.TryCreate(command.FirstName ?? "", false);
        var (lastName, lastNameError) = LastName.TryCreate(command.LastName ?? "", false);
        var (phoneNumber, phoneNumberError) = CandidatePhoneNumber.TryCreate(command.Phone);

        var errors = new List<string>();
        if (emailError != null) errors.Add(emailError);
        if (phoneNumberError != null) errors.Add(phoneNumberError);

        return errors.Count > 0 ? throw new ValidationException(errors) : (email!, firstName!, lastName!, phoneNumber!);
    }
}