namespace HrAgencySystem.SharedKernel.Snapshots;

public sealed record JobDescriptionSnapshot(Guid JobDescriptionId, string JobDescriptionTitle, Guid CompanyId);