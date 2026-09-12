using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Update;

public static class UpdateJobApplicationHandler
{

    [AggregateHandler]
    public static async Task<(JobApplicationUpdated, Wolverine.Marten.Events)> Handle(
        UpdateJobApplication command,
        JobApplication aggregate,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct)
    {
        var now = clock.UtcNow;
        
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId.Value);
        
        var (firstName, lastName, phoneNumber) = GetValueObjects(command);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new JobApplicationUpdated(command.ApplicationId,
            now, firstName.Value, lastName.Value, phoneNumber.Value, user);

        return (@event, [@event]);
    }
    
    private static (FirstName firstName, LastName lastName, CandidatePhoneNumber phoneNumber)
        GetValueObjects(UpdateJobApplication command)
    {
        var (firstName, firstNameError) = FirstName.TryCreate(command.FirstName ?? "", false);
        var (lastName, lastNameError) = LastName.TryCreate(command.LastName ?? "", false);
        var (phoneNumber, phoneNumberError) = CandidatePhoneNumber.TryCreate(command.Phone);

        var errors = new List<string>();
        if (phoneNumberError != null) errors.Add(phoneNumberError);
        if (firstNameError != null) errors.Add(firstNameError);
        if (lastNameError != null) errors.Add(lastNameError);

        return errors.Count > 0 ? throw new ValidationException(errors) : (firstName!, lastName!, phoneNumber!);
    }
}