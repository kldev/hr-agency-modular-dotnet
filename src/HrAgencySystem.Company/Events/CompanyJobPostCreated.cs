namespace HrAgencySystem.Company.Events;

public sealed record CompanyJobPostCreated(Guid CompanyId, Guid JobPostId);