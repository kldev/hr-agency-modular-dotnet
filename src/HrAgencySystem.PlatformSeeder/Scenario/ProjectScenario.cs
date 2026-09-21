using HrAgencySystem.Company.Application.CompleteProfile;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Application.ChangeStatus;
using HrAgencySystem.Projects.Application.Contacts.Assign;
using HrAgencySystem.Projects.Application.Contract.Record;
using HrAgencySystem.Projects.Application.Create;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Web.Common;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// Deliveries to work against. The set is chosen for what it makes visible rather than for realism:
/// Germany and Belgium under different engagement types, because the compliance catalogue is keyed
/// on exactly that pair, and one Polish project because posting inside the country of establishment
/// raises no host-state duties - an empty checklist is an answer the screens have to render too.
/// </summary>
internal sealed class ProjectScenario(IMessageBus bus, Func<Task> waitForProjections)
{
    internal sealed record ProjectData(
        Guid ProjectId,
        string Name,
        string WorkCountry,
        EngagementType EngagementType,
        DateOnly StartsOn,
        DateOnly? EndsOn
    );

    private sealed record Spec(
        string Name,
        string Description,
        EngagementType EngagementType,
        string Street,
        string BuildingNumber,
        string PostalCode,
        string City,
        string CountryCode,
        int StartOffsetMonths,
        int? EndOffsetMonths,
        /// <summary>
        /// Whether to carry this one all the way to Active. Going live needs a signed contract, a
        /// responsible contact and a complete client profile, so two are left in Draft on purpose -
        /// that is the state the go-live checklist exists to explain.
        /// </summary>
        bool GoLive
    );

    private static readonly Spec[] Specs =
    [
        new(
            "Munich core banking rollout",
            "Backend engineers delivering a core banking migration on the client's site in Munich.",
            EngagementType.PostingOfWorkers,
            "Leopoldstraße",
            "154",
            "80804",
            "München",
            "DE",
            -8,
            10,
            true
        ),
        new(
            "Hamburg logistics platform",
            "Developers hired out to the client, who directs their work. The heavy AÜG variant.",
            EngagementType.TemporaryAgencyWork,
            "Willy-Brandt-Straße",
            "23",
            "20457",
            "Hamburg",
            "DE",
            -5,
            14,
            true
        ),
        new(
            "Berlin data platform (local hires)",
            "People employed under German law through a local entity - no posting, so no A1.",
            EngagementType.LocalEmployment,
            "Friedrichstraße",
            "68",
            "10117",
            "Berlin",
            "DE",
            -3,
            18,
            false
        ),
        new(
            "Brussels payments integration",
            "Integration team posted to Belgium. Limosa territory.",
            EngagementType.PostingOfWorkers,
            "Rue de la Loi",
            "155",
            "1040",
            "Bruxelles",
            "BE",
            -6,
            12,
            true
        ),
        new(
            "Kraków internal tooling",
            "Domestic delivery. No host-state duties, and the compliance list is empty on purpose.",
            EngagementType.PostingOfWorkers,
            "Wielicka",
            "28",
            "30-552",
            "Kraków",
            "PL",
            -4,
            null,
            false
        ),
    ];

    internal async Task<IReadOnlyList<ProjectData>> Create(
        Guid organizationId,
        IReadOnlyList<Guid> companyIds,
        IReadOnlyList<LegalEntityScenario.LegalEntityData> legalEntities,
        IReadOnlyList<Guid> userIds,
        Guid? teamId,
        DateOnly today
    )
    {
        var created = new List<ProjectData>();
        var pending = new List<PendingGoLive>();

        for (var index = 0; index < Specs.Length; index++)
        {
            var spec = Specs[index];
            var startsOn = today.AddMonths(spec.StartOffsetMonths);
            var endsOn = spec.EndOffsetMonths is null
                ? (DateOnly?)null
                : today.AddMonths(spec.EndOffsetMonths.Value);

            var command = new CreateProject(
                organizationId,
                companyIds[index % companyIds.Count],
                legalEntities[index % legalEntities.Count].LegalEntityId,
                spec.Name,
                spec.Description,
                spec.EngagementType,
                spec.Street,
                spec.BuildingNumber,
                null,
                spec.PostalCode,
                spec.City,
                spec.CountryCode,
                startsOn,
                endsOn,
                teamId,
                userIds[index % userIds.Count]
            );

            var result = await bus.InvokeAsync<ProjectCreated>(command);

            if (spec.GoLive)
                pending.Add(
                    new PendingGoLive(
                        result.ProjectId,
                        command.CompanyId,
                        result.Company.Name,
                        userIds[index % userIds.Count],
                        startsOn,
                        endsOn
                    )
                );

            created.Add(
                new ProjectData(
                    result.ProjectId,
                    spec.Name,
                    spec.CountryCode,
                    spec.EngagementType,
                    startsOn,
                    endsOn
                )
            );
        }

        // Completing the client profile has to land in the projection before a contract is recorded
        // against it. The snapshot repository does fall back to replaying the stream, but only when
        // the projection is missing entirely - here the company was projected long ago and would
        // simply answer with the stale "profile incomplete".
        foreach (var goLive in pending)
            await CompleteClientProfile(organizationId, goLive);

        if (pending.Count > 0)
            await waitForProjections();

        foreach (var goLive in pending)
            await GoLive(organizationId, goLive);

        return created;
    }

    private sealed record PendingGoLive(
        Guid ProjectId,
        Guid CompanyId,
        string CompanyName,
        Guid ModifiedBy,
        DateOnly StartsOn,
        DateOnly? EndsOn
    );

    /// <summary>
    /// The client's paperwork. A profile counts as complete only once it has a legal name and a
    /// registered address, and both the contract and the go-live check refuse without them.
    /// </summary>
    private async Task CompleteClientProfile(Guid organizationId, PendingGoLive goLive)
    {
        await bus.InvokeAsync<CompanyProfileCompleted>(
            new CompleteCompanyProfile(
                goLive.CompanyId,
                organizationId,
                $"{goLive.CompanyName} sp. z o.o.",
                "Aleje Jerozolimskie",
                "96",
                null,
                "00-807",
                "Warszawa",
                "PL",
                null,
                "PL83101010230000261395100000",
                "NBPLPLPW",
                new ContactPerson(
                    "legal@client.example.com",
                    "Katarzyna",
                    "Dąbrowska",
                    "Board member",
                    "+48 22 550 11 20"
                ),
                goLive.ModifiedBy
            )
        );
    }

    /// <summary>
    /// The rest of what the domain asks for before a project may go live, then the status change
    /// itself. Done in the same order a person would do it, because there is no shortcut command.
    /// </summary>
    private async Task GoLive(Guid organizationId, PendingGoLive goLive)
    {
        await bus.InvokeAsync<ProjectContactAssigned>(
            new AssignProjectContact(
                goLive.ProjectId,
                organizationId,
                ContactRole.Responsible,
                new ContactPerson(
                    "delivery@client.example.com",
                    "Michał",
                    "Grabowski",
                    "Delivery manager",
                    "+48 22 550 11 44"
                ),
                null,
                goLive.ModifiedBy
            )
        );

        await bus.InvokeAsync<ProjectContractRecorded>(
            new RecordProjectContract(
                goLive.ProjectId,
                organizationId,
                $"UM/{goLive.StartsOn:yyyy}/{goLive.ProjectId.ToString()[..4].ToUpperInvariant()}",
                ContractStatus.Signed,
                goLive.StartsOn.AddDays(-14),
                goLive.StartsOn,
                goLive.EndsOn,
                goLive.ModifiedBy
            )
        );

        await bus.InvokeAsync<ProjectStatusChanged>(
            new ChangeProjectStatus(
                goLive.ProjectId,
                organizationId,
                ProjectStatus.Active,
                "Contract signed, delivery started.",
                goLive.ModifiedBy
            )
        );
    }
}
