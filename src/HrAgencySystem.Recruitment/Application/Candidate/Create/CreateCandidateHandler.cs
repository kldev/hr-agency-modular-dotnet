using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Recruitment.Application.Candidate.Create;

public static class CreateCandidateHandler
{
    public static async Task<CandidateCreated> Handle(
        CreateCandidate command, 
        IOrganizationChecker checker, 
        ICandidateEmailReservationRepository repository, 
        IUserSnapshotRepository snapshotRepository,
        IDocumentSession session,
        IClock clock, 
        CancellationToken ct)
    {
        var (email, phone, firstName, lastName) = CreateValueObjects(command);
        
        var organizationId = OrganizationId.From(command.OrganizationId);

        await ValidateOrganization(command, checker, ct);

        var candidateId = CandidateId.New();

        await ValidateEmailReservation(repository, organizationId, email, ct);

        await repository.ReserveAsync(organizationId, email!, candidateId);

        var createdBy = await GetCreatedBy(command, snapshotRepository, ct);

        var @event = new CandidateCreated(
            candidateId.Value, 
            organizationId.Value, 
            email.Value, 
            phone.Value,
            command.Source, 
            clock.UtcNow, 
            createdBy,
            command.CompanyId,
            firstName.Value,
            lastName.Value);

        session.Events.StartStream<Domain.Candidates.Candidate>(candidateId.Value, @event);
        
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
        OrganizationId organizationId, Email email, CancellationToken ct)
    {
        var reserved = await repository.ExistsAsync(organizationId, email, ct);
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

    private static (Email email, CandidatePhoneNumber phone, FirstName firstName, LastName lastName) CreateValueObjects(
        CreateCandidate command)
    {
        var (email, error) = Email.TryCreate(command.Email);
        var (phone, phoneError) = CandidatePhoneNumber.TryCreate(command.Phone);
        var errors = new List<string>();
        if (error != null) errors.Add(error);
        if (phoneError != null) errors.Add(phoneError);

        var (firstName, _) = FirstName.TryCreate(command.FirstName ?? "", false);
        var (lastName, _) = LastName.TryCreate(command.LastName ?? "", false);
        
        return errors.Count > 0 ? throw new ValidationException(errors) : (email!, phone!, firstName!, lastName!);
    }
}