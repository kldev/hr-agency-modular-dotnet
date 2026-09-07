namespace HrAgencySystem.Company.Events;

public sealed record CompanyJobPostActiveChanged(Guid JobPostId, Guid CompanyId, int ChangeBy);