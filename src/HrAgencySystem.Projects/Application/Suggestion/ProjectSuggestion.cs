using HrAgencySystem.Projects.Domain;

namespace HrAgencySystem.Projects.Application.Suggestion;

public sealed record ProjectSuggestion(
    Guid Id,
    string Name,
    string CompanyName,
    ProjectStatus Status
);
