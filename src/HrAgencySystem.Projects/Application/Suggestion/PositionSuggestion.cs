using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Application.Suggestion;

/// <summary>
/// One line in a picker. Carries the internal name, because that is what tells two roles apart -
/// "Painter Belgium" and "Painter PL contract" are both called "Painter" on the document.
/// <para>
/// The engagement type and the contract type ride along so the assignment wizard can fill itself
/// in the moment somebody picks a role, instead of asking again for what the role already says.
/// </para>
/// </summary>
public sealed record PositionSuggestion(
    Guid Id,
    Guid ProjectId,
    string Name,
    string ContractName,
    WorkerContractType ContractType,
    EngagementType? DefaultEngagementType,
    int? PlannedHeadcount,
    int AssignedCount
);
