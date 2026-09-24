using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// A role opened inside a project: what somebody actually does there, on what terms and where.
/// <para>
/// One client, one company, one project - and inside it painters and bricklayers, which are two
/// different jobs with two different rates, two different sets of duties and often two different
/// contracts. That is why a position belongs to the project rather than to an organization-wide
/// dictionary: it exists only as part of this delivery, and it dies with it.
/// </para>
/// <para>
/// The fields past the name are the ones a contract asks for. Nothing here generates a document
/// yet; the point is that when something does, every field it needs is already answered in one
/// place instead of being retyped per person.
/// </para>
/// </summary>
public sealed record ProjectPosition(
    Guid PositionId,
    // Internal name, unique within the project: "Painter PL contract", "Painter Belgium".
    string Name,
    // The name that goes on the document, without our own bookkeeping: "Painter".
    string ContractName,
    string WorkDescription,
    IReadOnlyList<string> Duties,
    IReadOnlyList<string> RequiredQualifications,
    WorkerContractType ContractType,
    WorkRate? ProposedRate,
    // Null means the project's own workplace: one delivery often runs on several sites.
    PostalAddress? WorkplaceAddress,
    decimal? WeeklyHours,
    TimeOnly? WorkStartsAt,
    string WorkSchedule,
    // Day of the month wages are paid by; on every contract, and the first thing people ask.
    int? PayoutDay,
    string ProbationPeriod,
    string NoticePeriod,
    // Accommodation, transport, per diem - named, because on a posting these are half the offer.
    IReadOnlyList<string> Allowances,
    // The target headcount for this role. Deliberately not a forecast: what a client says they
    // will need in two months is a sales matter and a different entity entirely.
    int? PlannedHeadcount,
    // A hint for the assignment wizard. The engagement type is still stated per person, because
    // it is what keys their compliance.
    EngagementType? DefaultEngagementType,
    bool IsArchived,
    UserSnapshot OpenedBy,
    DateTimeOffset OpenedAt,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
)
{
    /// <summary>
    /// Where the work is actually done. A position without its own address inherits the project's,
    /// so no caller has to know which of the two answered.
    /// </summary>
    public PostalAddress WorkplaceAddressOr(PostalAddress projectAddress) =>
        WorkplaceAddress ?? projectAddress;
}
