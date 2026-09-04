namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public record CandidateEmailReservation(Guid Id, Guid OrganizationId, Guid CandidateId, string Email);
