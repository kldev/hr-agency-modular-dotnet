namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

// ReSharper disable once ClassNeverInstantiated.Global
public record CandidateEmailReservation(Guid Id, Guid OrganizationId, Guid CandidateId, string Email);
