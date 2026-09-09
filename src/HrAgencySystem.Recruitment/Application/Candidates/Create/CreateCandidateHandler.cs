using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Domain.Candidates.ValueObjects;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Extensions;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Recruitment.Application.Candidates.Create;

public static class CreateCandidateHandler
{
    public static async Task<CandidateCreated> Handle(
        CreateCandidate command, 
        IRecruitmentService service, 
        ICandidateEmailReservationRepository repository, 
        IDocumentSession session,
        IClock clock, 
        CancellationToken ct)
    {
        var (email, phone, firstName, lastName, note) = CreateValueObjects(command);
        
        var organizationId = OrganizationId.From(command.OrganizationId);

        await  service.ValidateOrganization(command.OrganizationId, ct);

        var candidateId = CandidateId.New();

        await ValidateEmailReservation(repository, organizationId, email, ct);

        await repository.ReserveAsync(organizationId, email!, candidateId);

        UserSnapshot? createdBy = await GetUser(service, command, ct);

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
            lastName.Value, 
            note.Value
            );

        session.Events.StartStream<Candidate>(candidateId.Value, @event);
        
        return @event;
    }

    private static async Task<UserSnapshot?> GetUser(IRecruitmentService service, CreateCandidate command, CancellationToken ct)
    {
        if (command.CreatedBy.IsInvalid()) return null;

        var result = await service.GetUserAsync(command.CreatedBy!.Value, ct);
        return (UserSnapshot?)result;
    }
    
    private static async Task ValidateEmailReservation(ICandidateEmailReservationRepository repository,
        OrganizationId organizationId, Email email, CancellationToken ct)
    {
        var reserved = await repository.ExistsAsync(organizationId, email, ct);
        if (reserved)
            throw new BusinessRuleException(ICandidateEmailReservationRepository.EmailAlreadyExistsMessage);
    }
    
    private static (Email email, 
        CandidatePhoneNumber phone, 
        FirstName 
        firstName, 
        LastName lastName,
        LongText note) CreateValueObjects(
        CreateCandidate command)
    {
        var (data, errors) = CandidateDataFactory.Create(command, true);
        var (email, error) = Email.TryCreate(command.Email);
        if (error != null) errors.Add(error);
        
        return errors.Count > 0 ? throw new ValidationException(errors) 
            : (email!, data.Phone, data.FirstName, data.LastName, data.Note);
    }
}