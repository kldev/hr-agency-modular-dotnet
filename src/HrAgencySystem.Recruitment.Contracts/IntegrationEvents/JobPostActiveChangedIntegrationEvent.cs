namespace HrAgencySystem.Recruitment.Contracts.IntegrationEvents;

public sealed record JobPostActiveChangedIntegrationEvent(Guid JobPostId, Guid CompanyId, int ChangeBy);