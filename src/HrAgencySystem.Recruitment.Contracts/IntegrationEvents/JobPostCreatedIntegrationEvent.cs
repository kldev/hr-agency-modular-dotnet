namespace HrAgencySystem.Recruitment.Contracts.IntegrationEvents;

public sealed record JobPostCreatedIntegrationEvent(Guid CompanyId, Guid JobPostId);

