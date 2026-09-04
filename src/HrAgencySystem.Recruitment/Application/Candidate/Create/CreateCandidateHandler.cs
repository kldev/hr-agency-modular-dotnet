using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Domain.Candidate.ValueObjects;
using HrAgencySystem.Recruitment.Events.Candidate;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.Candidate.Create;

public static class CreateCandidateHandler
{
    public static async Task<CandidateCreated> Handle(
        CreateCandidate command, 
        IOrganizationChecker checker, 
        ICandidateEmailReservationRepository repository, 
        IUserSnapshotRepository snapshotRepository,
        IClock clock, 
        CancellationToken ct)
    {
        var (email, phone) = CreateValueObjects(command);
        
        var organizationId = OrganizationId.From(command.OrganizationId);

        await ValidateOrganization(command, checker, ct);

        var candidateId = CandidateId.New();

        await ValidateEmailReservation(repository, ct, organizationId, email);

        await repository.ReserveAsync(organizationId, email!, candidateId, ct);

        var createdBy = await GetCreatedBy(command, snapshotRepository, ct);

        var @event = new CandidateCreated(
            candidateId.Value, 
            organizationId.Value, 
            email.Value, 
            phone.Value,
            command.Source, 
            clock.UtcNow, 
            createdBy,
            command.CompanyId);

        return @event;
    }

    private static async Task<UserSnapshot?> GetCreatedBy(CreateCandidate command, IUserSnapshotRepository snapshotRepository,
        CancellationToken ct)
    {
        UserSnapshot? createdBy = null;
        if (command.CreatedBy != null && command.CreatedBy != Guid.Empty)
        {
            createdBy = await snapshotRepository.GetUserAsync(command.CreatedBy.Value, ct);
        }

        return createdBy;
    }

    private static async Task ValidateEmailReservation(ICandidateEmailReservationRepository repository,
        CancellationToken ct, OrganizationId organizationId, Email email)
    {
        var reserved = await repository.ExistsAsync(organizationId, email!, ct);
        if (reserved)
            throw new BusinessRuleException(ICandidateEmailReservationRepository.EmailAlreadyExistsMessage);
    }

    private static async Task ValidateOrganization(CreateCandidate command, IOrganizationChecker checker,
        CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(command.OrganizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    private static (Email email, CandidatePhoneNumber phone) CreateValueObjects(CreateCandidate command)
    {
        var (email, error) = Email.TryCreate(command.Email);
        var (phone, phoneError) = CandidatePhoneNumber.TryCreate(command.PhoneNumber);
        var errors = new List<string>();
        if (error != null) errors.Add(error);
        if (phoneError != null) errors.Add(phoneError);

        return errors.Count > 0 ? throw new ValidationException(errors) : (email!, phone!);
    }
}