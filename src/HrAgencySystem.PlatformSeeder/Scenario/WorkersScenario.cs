using HrAgencySystem.Compliance;
using HrAgencySystem.Workers.Application.AssignmentCompliance.Record;
using HrAgencySystem.Workers.Application.ChangeAssignmentStatus;
using HrAgencySystem.Workers.Application.ChangeWorkerStatus;
using HrAgencySystem.Workers.Application.PlanAssignment;
using HrAgencySystem.Workers.Application.RegisterWorker;
using HrAgencySystem.Workers.Application.WorkAuthorisations.Record;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// The register of people and of where they work. The cast is written out by hand rather than faked,
/// because the point of this data is the awkward cases: somebody whose identity document has expired
/// and therefore cannot be marked as employed, somebody stuck in legalisation, somebody with two
/// postings so that their history is a history, and five people with no posting at all - they are
/// what makes the split of the register into "here" and "abroad" worth looking at.
/// </summary>
internal sealed class WorkersScenario(IMessageBus bus, Func<Task> waitForProjections)
{
    private enum ComplianceFill
    {
        /// <summary>Nothing recorded - the checklist shows the catalogue as Not started.</summary>
        None,

        /// <summary>Half the list answered, the rest left open.</summary>
        Partial,

        /// <summary>Everything confirmed, well inside its validity.</summary>
        Full,

        /// <summary>Confirmed, but the first item runs out in three weeks.</summary>
        Expiring,

        /// <summary>One item already ran out and says so.</summary>
        Lapsed,
    }

    private sealed record AuthorisationSpec(
        WorkAuthorisationKind Kind,
        string Country,
        string Number,
        int ValidFromOffsetMonths,
        int ValidUntilOffsetMonths
    );

    private sealed record AssignmentSpec(
        int ProjectIndex,
        string Position,
        int StartOffsetMonths,
        int? EndOffsetMonths,
        AssignmentStatus TargetStatus,
        ComplianceFill Compliance
    );

    private sealed record WorkerSpec(
        string FirstName,
        string LastName,
        int BirthYear,
        int BirthMonth,
        int BirthDay,
        string Citizenship,
        IdentityDocumentKind DocumentKind,
        string DocumentNumber,
        string DocumentIssuingCountry,
        /// <summary>Months from today. Negative means the document has already run out.</summary>
        int? DocumentValidUntilOffsetMonths,
        string? Email,
        string? PhoneNumber,
        WorkerStatus TargetStatus,
        IReadOnlyList<AuthorisationSpec> Authorisations,
        IReadOnlyList<AssignmentSpec> Assignments,
        string? Note = null
    );

    private static readonly WorkerSpec[] Specs =
    [
        new(
            "Piotr",
            "Kowalski",
            1988,
            4,
            12,
            "PL",
            IdentityDocumentKind.IdentityCard,
            "ABC450192",
            "PL",
            40,
            "piotr.kowalski@example.com",
            "+48 601 220 145",
            WorkerStatus.Employed,
            [],
            [
                new AssignmentSpec(
                    0,
                    "Senior Java Developer",
                    -7,
                    9,
                    AssignmentStatus.Active,
                    ComplianceFill.Full
                ),
            ]
        ),
        new(
            "Anna",
            "Nowak",
            1991,
            9,
            3,
            "PL",
            IdentityDocumentKind.IdentityCard,
            "CDF882301",
            "PL",
            26,
            "anna.nowak@example.com",
            "+48 602 887 310",
            WorkerStatus.Employed,
            [],
            [
                new AssignmentSpec(
                    0,
                    "Business Analyst",
                    -6,
                    9,
                    AssignmentStatus.Active,
                    ComplianceFill.Expiring
                ),
            ]
        ),
        new(
            "Marek",
            "Wiśniewski",
            1985,
            1,
            28,
            "PL",
            IdentityDocumentKind.Passport,
            "EK7712045",
            "PL",
            33,
            "marek.wisniewski@example.com",
            "+48 604 115 902",
            WorkerStatus.Employed,
            [],
            [
                new AssignmentSpec(
                    3,
                    "Integration Engineer",
                    -5,
                    11,
                    AssignmentStatus.Active,
                    ComplianceFill.Partial
                ),
            ]
        ),
        new(
            "Oleksandr",
            "Tkachenko",
            1990,
            6,
            17,
            "UA",
            IdentityDocumentKind.Passport,
            "FE448210",
            "UA",
            29,
            "oleksandr.tkachenko@example.com",
            "+48 730 441 208",
            WorkerStatus.Employed,
            [
                new AuthorisationSpec(
                    WorkAuthorisationKind.WorkPermit,
                    "PL",
                    "PL/WP/2025/44821",
                    -14,
                    22
                ),
                new AuthorisationSpec(
                    WorkAuthorisationKind.ResidencePermit,
                    "PL",
                    "PL/RP/2025/10934",
                    -14,
                    28
                ),
            ],
            [
                new AssignmentSpec(
                    1,
                    "Backend Developer",
                    -4,
                    13,
                    AssignmentStatus.Active,
                    ComplianceFill.Full
                ),
            ]
        ),
        new(
            "Kateryna",
            "Shevchenko",
            1993,
            11,
            8,
            "UA",
            IdentityDocumentKind.Passport,
            "FE901337",
            "UA",
            19,
            "kateryna.shevchenko@example.com",
            "+48 731 900 512",
            WorkerStatus.Employed,
            [
                new AuthorisationSpec(
                    WorkAuthorisationKind.WorkPermit,
                    "PL",
                    "PL/WP/2025/90133",
                    -11,
                    13
                ),
            ],
            [
                new AssignmentSpec(
                    0,
                    "QA Engineer",
                    -3,
                    9,
                    AssignmentStatus.Active,
                    ComplianceFill.Partial
                ),
            ]
        ),
        new(
            "Mykola",
            "Bondarenko",
            1987,
            2,
            14,
            "UA",
            IdentityDocumentKind.Passport,
            "FE223981",
            "UA",
            15,
            "mykola.bondarenko@example.com",
            "+48 732 118 470",
            WorkerStatus.Legalisation,
            [new AuthorisationSpec(WorkAuthorisationKind.Visa, "PL", "PL/VIS/2026/22398", -2, 10)],
            [],
            "Waiting on the work permit decision."
        ),
        new(
            "Dmytro",
            "Kovalenko",
            1995,
            7,
            21,
            "UA",
            IdentityDocumentKind.Passport,
            "FE556104",
            "UA",
            23,
            "dmytro.kovalenko@example.com",
            "+48 733 620 991",
            WorkerStatus.ContractPreparation,
            [],
            []
        ),
        new(
            "Ivan",
            "Petrov",
            1989,
            3,
            30,
            "BY",
            IdentityDocumentKind.Passport,
            "MP4471203",
            "BY",
            12,
            "ivan.petrov@example.com",
            "+48 734 005 118",
            WorkerStatus.Legalisation,
            // Runs out in a month: this is the row the expiry column exists for.
            [
                new AuthorisationSpec(
                    WorkAuthorisationKind.WorkPermit,
                    "PL",
                    "PL/WP/2024/44712",
                    -22,
                    1
                ),
            ],
            []
        ),
        new(
            "Rajesh",
            "Kumar",
            1992,
            12,
            5,
            "IN",
            IdentityDocumentKind.Passport,
            "Z4881205",
            "IN",
            // Already expired, which is why this person is stuck at Onboarding: the domain refuses
            // to mark somebody employed on a lapsed identity document.
            -2,
            "rajesh.kumar@example.com",
            "+48 735 447 220",
            WorkerStatus.Onboarding,
            [
                new AuthorisationSpec(
                    WorkAuthorisationKind.WorkPermit,
                    "PL",
                    "PL/WP/2025/48812",
                    -8,
                    16
                ),
            ],
            [],
            "Passport renewal in progress; cannot be employed until it is on file."
        ),
        new(
            "Elena",
            "Popescu",
            1994,
            5,
            19,
            "RO",
            IdentityDocumentKind.IdentityCard,
            "RT884120",
            "RO",
            31,
            "elena.popescu@example.com",
            "+40 721 445 900",
            WorkerStatus.Employed,
            [],
            [
                new AssignmentSpec(
                    4,
                    "Frontend Developer",
                    -3,
                    null,
                    AssignmentStatus.Active,
                    ComplianceFill.None
                ),
            ]
        ),
        new(
            "Tomáš",
            "Novák",
            1986,
            8,
            24,
            "CZ",
            IdentityDocumentKind.IdentityCard,
            "CZ9920184",
            "CZ",
            27,
            "tomas.novak@example.com",
            "+420 602 118 330",
            WorkerStatus.Employed,
            [],
            [
                new AssignmentSpec(
                    2,
                    "Data Engineer",
                    -2,
                    17,
                    AssignmentStatus.Active,
                    ComplianceFill.Full
                ),
            ]
        ),
        new(
            "Sofia",
            "Ionescu",
            1997,
            10,
            2,
            "RO",
            IdentityDocumentKind.IdentityCard,
            "RT110293",
            "RO",
            35,
            "sofia.ionescu@example.com",
            "+40 722 880 145",
            WorkerStatus.Recruitment,
            [],
            []
        ),
        new(
            "Jakub",
            "Zieliński",
            1990,
            1,
            9,
            "PL",
            IdentityDocumentKind.IdentityCard,
            "GHK220417",
            "PL",
            38,
            "jakub.zielinski@example.com",
            "+48 605 330 271",
            WorkerStatus.Employed,
            [],
            [
                // The pair that makes a history: Belgium finished, Germany running.
                new AssignmentSpec(
                    3,
                    "DevOps Engineer",
                    -6,
                    -1,
                    AssignmentStatus.Completed,
                    ComplianceFill.Full
                ),
                new AssignmentSpec(
                    0,
                    "DevOps Engineer",
                    0,
                    9,
                    AssignmentStatus.Active,
                    ComplianceFill.Lapsed
                ),
            ]
        ),
        new(
            "Andrii",
            "Melnyk",
            1984,
            4,
            27,
            "UA",
            IdentityDocumentKind.Passport,
            "FE118206",
            "UA",
            17,
            "andrii.melnyk@example.com",
            "+48 736 220 884",
            WorkerStatus.Terminated,
            [
                new AuthorisationSpec(
                    WorkAuthorisationKind.WorkPermit,
                    "PL",
                    "PL/WP/2024/11820",
                    -20,
                    4
                ),
            ],
            [
                new AssignmentSpec(
                    1,
                    "Support Engineer",
                    -5,
                    -2,
                    AssignmentStatus.Interrupted,
                    ComplianceFill.Partial
                ),
            ],
            "Left of their own accord halfway through the posting."
        ),
        new(
            "Lucas",
            "Dubois",
            1996,
            6,
            11,
            "FR",
            IdentityDocumentKind.IdentityCard,
            "FR7741209",
            "FR",
            30,
            "lucas.dubois@example.com",
            "+33 6 12 88 40 15",
            WorkerStatus.ProjectChange,
            [],
            [
                // Never turned up. Deliberately not the same fact as breaking off early.
                new AssignmentSpec(
                    1,
                    "Mobile Developer",
                    -2,
                    12,
                    AssignmentStatus.DidNotStart,
                    ComplianceFill.None
                ),
                new AssignmentSpec(
                    2,
                    "Mobile Developer",
                    1,
                    17,
                    AssignmentStatus.Planned,
                    ComplianceFill.None
                ),
            ]
        ),
    ];

    internal async Task<int> Seed(
        Guid organizationId,
        IReadOnlyList<ProjectScenario.ProjectData> projects,
        IReadOnlyList<Guid> userIds,
        DateOnly today
    )
    {
        var createdBy = userIds[0];
        var registered = new List<(WorkerSpec Spec, Guid WorkerId)>();

        foreach (var spec in Specs)
        {
            var result = await bus.InvokeAsync<WorkerRegistered>(
                new RegisterWorker(
                    organizationId,
                    spec.FirstName,
                    spec.LastName,
                    new DateOnly(spec.BirthYear, spec.BirthMonth, spec.BirthDay),
                    spec.Citizenship,
                    spec.DocumentKind,
                    spec.DocumentNumber,
                    spec.DocumentIssuingCountry,
                    spec.DocumentValidUntilOffsetMonths is null
                        ? null
                        : today.AddMonths(spec.DocumentValidUntilOffsetMonths.Value),
                    spec.Email,
                    spec.PhoneNumber,
                    "Prosta",
                    "51",
                    null,
                    "00-838",
                    "Warszawa",
                    "PL",
                    spec.Note,
                    null,
                    null,
                    createdBy
                )
            );

            registered.Add((spec, result.WorkerId));
        }

        // The duplicate check on registration reads the projection, and so does every assignment
        // rule below. Nothing after this point is safe until the daemon has caught up.
        await waitForProjections();

        foreach (var (spec, workerId) in registered)
        {
            await WalkPipeline(organizationId, workerId, spec, createdBy);
            await RecordAuthorisations(organizationId, workerId, spec, createdBy, today);
        }

        await waitForProjections();

        // Assignments go in rounds rather than per person: the overlap check reads the projection,
        // so somebody's second posting may only be planned once their first is projected as closed.
        var rounds = Specs.Max(s => s.Assignments.Count);

        for (var round = 0; round < rounds; round++)
        {
            var planned =
                new List<(
                    AssignmentSpec Spec,
                    Guid AssignmentId,
                    ProjectScenario.ProjectData Project
                )>();

            foreach (var (spec, workerId) in registered)
            {
                if (spec.Assignments.Count <= round)
                    continue;

                var assignment = spec.Assignments[round];
                var project = projects[assignment.ProjectIndex];

                var result = await bus.InvokeAsync<AssignmentPlanned>(
                    new PlanAssignment(
                        organizationId,
                        workerId,
                        project.ProjectId,
                        project.EngagementType,
                        PositionOf(project, assignment.Position),
                        Clamp(today.AddMonths(assignment.StartOffsetMonths), project),
                        assignment.EndOffsetMonths is null
                            ? null
                            : Clamp(today.AddMonths(assignment.EndOffsetMonths.Value), project),
                        createdBy
                    )
                );

                planned.Add((assignment, result.AssignmentId, project));
            }

            await waitForProjections();

            foreach (var (assignment, assignmentId, project) in planned)
            {
                await MoveAssignment(
                    organizationId,
                    assignmentId,
                    assignment,
                    project,
                    createdBy,
                    today
                );
                await RecordCompliance(
                    organizationId,
                    assignmentId,
                    assignment,
                    project,
                    createdBy,
                    today
                );
            }

            await waitForProjections();
        }

        // Left until now: a terminated person cannot be assigned to anything, and an assignment
        // only goes active for somebody who is through the pipeline. Both facts read the same
        // status, so the final hop happens after the postings exist.
        foreach (var (spec, workerId) in registered)
        {
            if (spec.TargetStatus is WorkerStatus.Terminated or WorkerStatus.ProjectChange)
                await ChangeStatus(
                    organizationId,
                    workerId,
                    spec.TargetStatus,
                    createdBy,
                    spec.Note
                );
        }

        return registered.Count;
    }

    /// <summary>
    /// The period has to sit inside the project's own, which the offsets above only approximately
    /// respect - a demo is not worth a validation error.
    /// </summary>
    /// <summary>
    /// The role an assignment is planned against. Matched by the name the spec uses; when a spec
    /// names a role its project does not have, the project's first role stands in - a seeder that
    /// threw here would be one typo away from producing no data at all.
    /// </summary>
    private static Guid PositionOf(ProjectScenario.ProjectData project, string name) =>
        project
            .Positions.FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
            )
            ?.PositionId
        ?? project.Positions[0].PositionId;

    private static DateOnly Clamp(DateOnly date, ProjectScenario.ProjectData project)
    {
        if (date < project.StartsOn)
            return project.StartsOn;

        return project.EndsOn is not null && date > project.EndsOn.Value
            ? project.EndsOn.Value
            : date;
    }

    private async Task WalkPipeline(
        Guid organizationId,
        Guid workerId,
        WorkerSpec spec,
        Guid createdBy
    )
    {
        var requiresLegalisation = LegalisationPolicy.RequiresLegalisation(spec.Citizenship);

        // Terminated and ProjectChange are reached from Employed, and only once the postings exist.
        var target = spec.TargetStatus switch
        {
            WorkerStatus.Terminated or WorkerStatus.ProjectChange => WorkerStatus.Employed,
            var other => other,
        };

        foreach (var status in PathTo(target, requiresLegalisation))
            await ChangeStatus(organizationId, workerId, status, createdBy, null);
    }

    private static IEnumerable<WorkerStatus> PathTo(WorkerStatus target, bool requiresLegalisation)
    {
        if (target == WorkerStatus.Recruitment)
            yield break;

        yield return WorkerStatus.ContractPreparation;

        if (target == WorkerStatus.ContractPreparation)
            yield break;

        if (requiresLegalisation)
        {
            yield return WorkerStatus.Legalisation;

            if (target == WorkerStatus.Legalisation)
                yield break;
        }

        yield return WorkerStatus.Onboarding;

        if (target == WorkerStatus.Onboarding)
            yield break;

        yield return WorkerStatus.Employed;
    }

    private async Task ChangeStatus(
        Guid organizationId,
        Guid workerId,
        WorkerStatus status,
        Guid modifiedBy,
        string? reason
    ) =>
        await bus.InvokeAsync<WorkerStatusChanged>(
            new ChangeWorkerStatus(workerId, organizationId, status, reason, modifiedBy)
        );

    private async Task RecordAuthorisations(
        Guid organizationId,
        Guid workerId,
        WorkerSpec spec,
        Guid modifiedBy,
        DateOnly today
    )
    {
        foreach (var authorisation in spec.Authorisations)
            await bus.InvokeAsync<WorkAuthorisationRecorded>(
                new RecordWorkAuthorisation(
                    workerId,
                    organizationId,
                    null,
                    authorisation.Kind,
                    authorisation.Country,
                    authorisation.Number,
                    today.AddMonths(authorisation.ValidFromOffsetMonths),
                    today.AddMonths(authorisation.ValidUntilOffsetMonths),
                    null,
                    null,
                    modifiedBy
                )
            );
    }

    private async Task MoveAssignment(
        Guid organizationId,
        Guid assignmentId,
        AssignmentSpec spec,
        ProjectScenario.ProjectData project,
        Guid modifiedBy,
        DateOnly today
    )
    {
        if (spec.TargetStatus == AssignmentStatus.Planned)
            return;

        // Planned is where every assignment starts; Completed and Interrupted are reached through
        // Active, because the domain has no shortcut and neither should the seed.
        if (spec.TargetStatus is AssignmentStatus.Completed or AssignmentStatus.Interrupted)
            await ChangeAssignment(
                organizationId,
                assignmentId,
                AssignmentStatus.Active,
                null,
                modifiedBy
            );

        var endsOn = spec.TargetStatus switch
        {
            AssignmentStatus.Completed or AssignmentStatus.Interrupted => Clamp(
                today.AddMonths(spec.EndOffsetMonths ?? -1),
                project
            ),
            _ => (DateOnly?)null,
        };

        await ChangeAssignment(organizationId, assignmentId, spec.TargetStatus, endsOn, modifiedBy);
    }

    private async Task ChangeAssignment(
        Guid organizationId,
        Guid assignmentId,
        AssignmentStatus status,
        DateOnly? endsOn,
        Guid modifiedBy
    ) =>
        await bus.InvokeAsync<AssignmentStatusChanged>(
            new ChangeAssignmentStatus(
                assignmentId,
                organizationId,
                status,
                endsOn,
                null,
                modifiedBy
            )
        );

    private async Task RecordCompliance(
        Guid organizationId,
        Guid assignmentId,
        AssignmentSpec spec,
        ProjectScenario.ProjectData project,
        Guid modifiedBy,
        DateOnly today
    )
    {
        if (spec.Compliance == ComplianceFill.None)
            return;

        var requirements = ComplianceCatalogue.For(
            project.WorkCountry,
            project.EngagementType,
            ComplianceScope.Assignment
        );

        for (var index = 0; index < requirements.Count; index++)
        {
            var requirement = requirements[index];

            // Partial answers the first half and leaves the rest showing as Not started, which is
            // the state the checklist is actually for.
            if (spec.Compliance == ComplianceFill.Partial && index >= (requirements.Count + 1) / 2)
                continue;

            var (status, validTo) = Answer(spec.Compliance, index, today);

            await bus.InvokeAsync<AssignmentComplianceItemRecorded>(
                new RecordAssignmentComplianceItem(
                    assignmentId,
                    organizationId,
                    requirement,
                    status,
                    status == ComplianceStatus.Confirmed ? Reference(requirement, index) : null,
                    status == ComplianceStatus.NotStarted ? null : today.AddMonths(-2),
                    validTo,
                    null,
                    null,
                    modifiedBy
                )
            );
        }
    }

    private static (ComplianceStatus Status, DateOnly? ValidTo) Answer(
        ComplianceFill fill,
        int index,
        DateOnly today
    ) =>
        fill switch
        {
            // One item runs out in three weeks, so the expiry chip on the list has something to say.
            ComplianceFill.Expiring when index == 0 => (
                ComplianceStatus.Confirmed,
                today.AddDays(21)
            ),
            ComplianceFill.Lapsed when index == 0 => (ComplianceStatus.Expired, today.AddDays(-9)),
            ComplianceFill.Partial when index % 2 == 1 => (ComplianceStatus.InProgress, null),
            _ => (ComplianceStatus.Confirmed, today.AddMonths(14)),
        };

    private static string Reference(ComplianceRequirement requirement, int index) =>
        requirement switch
        {
            ComplianceRequirement.A1Certificates => $"A1/PL/2026/{4100 + index}",
            ComplianceRequirement.BeLimosaDeclaration => $"LIM-2026-{880_140 + index}",
            ComplianceRequirement.BeDimonaDeclaration => $"DIM-2026-{112_400 + index}",
            ComplianceRequirement.DeSocialSecurityRegistration => $"DE-SV-{7_740_100 + index}",
            ComplianceRequirement.DeLongTermPostingNotification => $"AENTG-2026-{5120 + index}",
            _ => $"REF-2026-{2200 + index}",
        };
}
