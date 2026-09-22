using HrAgencySystem.Compliance;
using HrAgencySystem.Workers.Domain;
using AssignmentMaps = HrAgencySystem.Api.Endpoints.Assignment.Maps;
using WorkerMaps = HrAgencySystem.Api.Endpoints.Worker.Maps;

namespace HrAgencySystem.IntegrationTests.Workers;

internal static class WorkerTestData
{
    public static readonly DateOnly StartsOn = new(2026, 10, 1);

    /// <summary>A Polish national: free movement, so legalisation has nothing to do.</summary>
    public const string PolishCitizenship = "pl";

    /// <summary>A Ukrainian national: a residence title and a permit come first.</summary>
    public const string UkrainianCitizenship = "ua";

    /// <summary>
    /// A fresh document number per call. The register refuses a second file for the same document,
    /// which is exactly the rule one of these tests is about - every other test needs to get past
    /// it without colliding with a neighbour.
    /// </summary>
    public static string NewDocumentNumber() => $"ZS{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    public static WorkerMaps.WorkerRequest RegisterRequest(
        string? documentNumber = null,
        string citizenship = PolishCitizenship,
        string firstName = "Jan",
        string lastName = "Kowalski",
        string issuingCountry = "pl",
        DateOnly? documentValidUntil = null,
        Guid? sourceCandidateId = null,
        string? email = null,
        string phoneNumber = "+48 600 000 000",
        Guid? sourceApplicationId = null
    ) =>
        new(
            firstName,
            lastName,
            new DateOnly(1990, 5, 12),
            citizenship,
            IdentityDocumentKind.Passport,
            documentNumber ?? NewDocumentNumber(),
            issuingCountry,
            documentValidUntil ?? new DateOnly(2032, 1, 1),
            email ?? EmailFor(firstName, lastName),
            phoneNumber,
            "Prosta",
            "51",
            null,
            "00-838",
            "Warszawa",
            "pl",
            null,
            sourceCandidateId,
            sourceApplicationId
        );

    /// <summary>
    /// Derived from the name so that two different people in one test do not look like one person
    /// to the register - which is exactly what it would otherwise conclude, and rightly so.
    /// </summary>
    public static string EmailFor(string firstName, string lastName) =>
        $"{firstName}.{lastName}@example.com".ToLowerInvariant();

    public static WorkerMaps.MapChangeStatus.ChangeWorkerStatusRequest WorkerStatusRequest(
        WorkerStatus status,
        string? reason = null
    ) => new(status, reason);

    public static WorkerMaps.MapRecordAuthorisation.RecordWorkAuthorisationRequest AuthorisationRequest(
        Guid? authorisationId = null,
        Guid? documentId = null,
        DateOnly? validUntil = null
    ) =>
        new(
            WorkAuthorisationKind.WorkPermit,
            "pl",
            "ZEZW/2026/123",
            new DateOnly(2026, 1, 1),
            validUntil ?? new DateOnly(2028, 12, 31),
            authorisationId,
            documentId
        );

    public static AssignmentMaps.MapPlan.PlanAssignmentRequest PlanRequest(
        Guid workerId,
        Guid projectId,
        Guid positionId,
        EngagementType engagementType = EngagementType.PostingOfWorkers,
        DateOnly? startsOn = null,
        DateOnly? endsOn = null
    ) => new(workerId, projectId, engagementType, positionId, startsOn ?? StartsOn, endsOn);

    public static AssignmentMaps.MapChangeStatus.ChangeAssignmentStatusRequest AssignmentStatusRequest(
        AssignmentStatus status,
        DateOnly? endsOn = null,
        string? reason = null
    ) => new(status, endsOn, reason);

    public static AssignmentMaps.MapRecordCompliance.RecordAssignmentComplianceItemRequest ComplianceRequest(
        ComplianceStatus status = ComplianceStatus.Confirmed,
        string? reference = null,
        DateOnly? validFrom = null,
        DateOnly? validTo = null,
        Guid? documentId = null
    ) => new(status, reference, validFrom, validTo, documentId);
}
