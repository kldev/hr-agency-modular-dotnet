using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Company.Events;

/// <summary>
/// Raised only on the crossing from incomplete to complete. Repeating it on every later edit would
/// make it mean "somebody saved the form", which is not a fact anybody needs recorded.
/// </summary>
public sealed record CompanyProfileCompleted(
    Guid CompanyId,
    Guid OrganizationId,
    UserSnapshot CompletedBy,
    DateTimeOffset CompletedAt
);
