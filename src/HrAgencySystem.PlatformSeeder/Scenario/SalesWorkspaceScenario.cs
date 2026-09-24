using HrAgencySystem.Company.Application.CompleteProfile;
using HrAgencySystem.Company.Application.Create;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.Compliance;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.Projects.Application.Create;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Sales.Application.Activities.Create;
using HrAgencySystem.Sales.Application.Opportunities.ChangeStage;
using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web.Common;
using HrAgencySystem.Tasks.Application.Complete;
using HrAgencySystem.Tasks.Application.Create;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

/// <summary>
/// The salesperson's workspace (plan 029): four named client companies with their deals, a history
/// of calls and meetings, projects sold from those deals, and a to-do list dated around today -
/// overdue, today, tomorrow, later this week and month, a few already done.
/// <para>
/// Two passes. The companies go first, so the delivery seed puts its staffed projects on them and
/// the workspace shows people on a project; the rest follows once those projects exist. Tasks are
/// given to the two accounts somebody logs in as: <c>bob.sale</c> and <c>j.smith</c> (end to end).
/// </para>
/// </summary>
internal sealed class SalesWorkspaceScenario(IMessageBus bus, IDocumentSession session, ISalesService sales)
{
    public const string AcmeName = "ACME Sp. z o.o.";

    private sealed record CompanySpec(string Name, string TaxId, string City, string Street, string Building, string PostalCode, Industry Industry, ContactPerson Contact);

    private static readonly CompanySpec[] Companies =
    [
        new(AcmeName, "5260001001", "Warszawa", "Aleje Jerozolimskie", "96", "00-807", Industry.Logistics,
            new ContactPerson("anna.kowalska@acme.example.com", "Anna", "Kowalska", "HR Manager", "+48 600 123 456")),
        new("Auto Parts Polska", "6340002002", "Katowice", "ul. Chorzowska", "50", "40-121", Industry.Manufacturing,
            new ContactPerson("p.wisniewski@autoparts.example.com", "Piotr", "Wiśniewski", "Plant Director", "+48 601 222 333")),
        new("Moto Service Group", "7790003003", "Poznań", "ul. Głogowska", "248", "60-104", Industry.Manufacturing,
            new ContactPerson("m.lewandowska@motoservice.example.com", "Magdalena", "Lewandowska", "Head of Purchasing", "+48 602 444 555")),
        new("Global Manufacturing", "8990004004", "Wrocław", "ul. Legnicka", "65", "54-206", Industry.Manufacturing,
            new ContactPerson("t.zielinski@globalmfg.example.com", "Tomasz", "Zieliński", "Operations Manager", "+48 603 666 777")),
    ];

    private sealed record DealSpec(int Company, string Title, string Description, decimal Value, OpportunityStage Stage, int CloseInDays);

    private static readonly DealSpec[] Deals =
    [
        new(0, "Recruitment Q4", "Twelve warehouse operatives for the Q4 peak.", 96_000, OpportunityStage.Proposal, 21),
        new(0, "Warehouse Workers", "Permanent team for the new Błonie warehouse.", 72_000, OpportunityStage.Won, -10),
        new(1, "Production Workers", "Line workers for the second shift.", 88_000, OpportunityStage.Qualified, 35),
        new(2, "Maintenance Team", "Mechanics and electricians for the service halls.", 54_000, OpportunityStage.Won, -30),
        new(3, "Production Workers Q1", "Seasonal staff for the January ramp-up.", 99_000, OpportunityStage.Contacted, 60),
    ];

    private sealed record ActivitySpec(int Deal, SalesActivityType Type, string Note, int DaysAgo, int Hour);

    private static readonly ActivitySpec[] Activities =
    [
        new(0, SalesActivityType.Call, "Discussed candidate requirements with Anna Kowalska.", 0, 10),
        new(0, SalesActivityType.Email, "Sent the recruitment proposal and the price list.", 1, 15),
        new(0, SalesActivityType.Meeting, "On-site meeting with the HR team, headcount confirmed.", 3, 11),
        new(1, SalesActivityType.Note, "Contract signed, first ten people start on Monday.", 12, 9),
        new(1, SalesActivityType.Presentation, "Presented the onboarding plan for the warehouse.", 18, 13),
        new(2, SalesActivityType.Call, "Plant director interested, asked for rates per shift.", 2, 14),
        new(2, SalesActivityType.Meeting, "Visited the production line in Katowice.", 6, 10),
        new(3, SalesActivityType.Email, "Confirmed the start date of the maintenance team.", 25, 8),
        new(4, SalesActivityType.Call, "First contact, they will send the headcount plan.", 4, 16),
    ];

    private sealed record TaskSpec(int Company, int? Deal, string Title, int DayOffset, int Hour, int Minute, TaskPriority Priority, bool Done);

    private static readonly TaskSpec[] TaskSpecs =
    [
        new(0, 0, "Follow up with purchasing", -2, 9, 0, TaskPriority.High, false),
        new(0, 0, "Call HR Manager", 0, 10, 30, TaskPriority.High, false),
        new(0, 0, "Send recruitment proposal", 0, 13, 0, TaskPriority.Medium, false),
        new(1, 2, "Prepare candidate report", 1, 9, 0, TaskPriority.Medium, false),
        new(2, 3, "Follow up contract", 2, 11, 0, TaskPriority.Low, false),
        new(3, 4, "Schedule client meeting", 4, 12, 0, TaskPriority.Medium, false),
        new(1, null, "Check the plant's safety training requirements", 12, 10, 0, TaskPriority.Low, false),
        new(0, 1, "Sent proposal to ACME", 0, 9, 0, TaskPriority.Medium, true),
        new(2, 3, "Called purchasing department", -1, 16, 30, TaskPriority.Low, true),
    ];

    private sealed record ProjectSpec(int Deal, string Name, string City, string Street, string PostalCode, int StartOffsetDays);

    private static readonly ProjectSpec[] Projects =
    [
        new(1, "Warehouse 2026", "Błonie", "ul. Fabryczna", "05-870", -7),
        new(0, "Production Workers Q4", "Warszawa", "ul. Postępu", "02-676", 30),
        new(3, "Maintenance Team", "Poznań", "ul. Głogowska", "60-104", -20),
    ];

    /// <summary>
    /// The four companies, profile included so the workspace shows where they are. Nothing when
    /// they are already there: tax ids are reserved, a second run would trip over the first.
    /// </summary>
    internal async Task<IReadOnlyList<Guid>> CreateCompanies(Guid organizationId, Guid createdBy)
    {
        if (await session.Query<CompanyProjection>().AnyAsync(c => c.OrganizationId == organizationId && c.Name == AcmeName))
            return [];

        var ids = new List<Guid>();

        foreach (var spec in Companies)
        {
            var created = await bus.InvokeAsync<CompanyCreated>(
                new CreateCompany(organizationId, spec.Name, "PL", spec.TaxId, "KRS" + spec.TaxId[..7], createdBy, spec.Industry, "", spec.Contact)
            );

            await bus.InvokeAsync<CompanyProfileCompleted>(
                new CompleteCompanyProfile(
                    created.CompanyId,
                    organizationId,
                    spec.Name,
                    spec.Street,
                    spec.Building,
                    null,
                    spec.PostalCode,
                    spec.City,
                    "PL",
                    "PL" + spec.TaxId,
                    "PL83101010230000261395100000",
                    "NBPLPLPW",
                    spec.Contact,
                    createdBy
                )
            );

            ids.Add(created.CompanyId);
        }

        return ids;
    }

    /// <summary>Deals, their history, the projects sold from them and the to-do lists.</summary>
    internal async Task<int> Seed(
        Guid organizationId,
        IReadOnlyList<Guid> companyIds,
        Guid legalEntityId,
        Func<Task> waitForProjections
    )
    {
        if (companyIds.Count < Companies.Length)
            return 0;

        var salesperson = await UserByEmail(organizationId, "bob.sale@");
        var demo = await UserByEmail(organizationId, "j.smith@");
        var owner = salesperson ?? demo;

        if (owner is null)
            return 0;

        var deals = new List<Guid>();
        foreach (var deal in Deals)
            deals.Add(await CreateDeal(organizationId, companyIds[deal.Company], deal, owner.Value));

        await waitForProjections();

        var now = DateTimeOffset.UtcNow;
        foreach (var activity in Activities)
        {
            var at = new DateTimeOffset(now.UtcDateTime.Date.AddDays(-activity.DaysAgo).AddHours(activity.Hour - 2), TimeSpan.Zero);

            await CreateActivityHandler.Handle(
                new CreateActivity(organizationId, deals[activity.Deal], activity.Type, activity.Note, owner.Value),
                sales,
                session,
                new FixedClock(at > now ? now : at),
                CancellationToken.None
            );
        }

        await session.SaveChangesAsync();

        foreach (var project in Projects)
            await CreateProject(organizationId, companyIds[Deals[project.Deal].Company], deals[project.Deal], legalEntityId, project, owner.Value);

        var count = 0;
        foreach (var person in new[] { salesperson, demo }.OfType<Guid>().Distinct())
            count += await CreateTasks(organizationId, companyIds, deals, person, now);

        return count;
    }

    private async Task<Guid> CreateDeal(Guid organizationId, Guid companyId, DealSpec deal, Guid owner)
    {
        var created = await bus.InvokeAsync<OpportunityCreated>(
            new CreateOpportunity(
                organizationId,
                companyId,
                deal.Title,
                deal.Description,
                deal.Value,
                deal.Stage == OpportunityStage.Proposal,
                CurrencyCode.PLN,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(deal.CloseInDays)),
                owner,
                owner
            )
        );

        if (deal.Stage != OpportunityStage.New)
            await bus.InvokeAsync<StageChanged>(
                new ChangeOpportunityStage(created.OpportunityId, organizationId, deal.Stage, "", owner)
            );

        return created.OpportunityId;
    }

    private async Task CreateProject(Guid organizationId, Guid companyId, Guid dealId, Guid legalEntityId, ProjectSpec spec, Guid owner)
    {
        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(spec.StartOffsetDays));

        await bus.InvokeAsync<ProjectCreated>(
            new CreateProject(
                organizationId,
                companyId,
                legalEntityId,
                spec.Name,
                "Sold as " + Deals[spec.Deal].Title,
                EngagementType.LocalEmployment,
                spec.Street,
                "1",
                null,
                spec.PostalCode,
                spec.City,
                "PL",
                startsOn,
                startsOn.AddMonths(12),
                null,
                owner,
                dealId
            )
        );
    }

    private async Task<int> CreateTasks(Guid organizationId, IReadOnlyList<Guid> companyIds, IReadOnlyList<Guid> deals, Guid person, DateTimeOffset now)
    {
        var warsaw = TimeZoneInfo.FindSystemTimeZoneById(TaskRange.DefaultTimeZone);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, warsaw).DateTime);

        foreach (var spec in TaskSpecs)
        {
            var local = today.AddDays(spec.DayOffset).ToDateTime(new TimeOnly(spec.Hour, spec.Minute));
            var dueAt = new DateTimeOffset(local, warsaw.GetUtcOffset(local));

            var created = await bus.InvokeAsync<TaskItemCreated>(
                new CreateTaskItem(
                    organizationId,
                    person,
                    companyIds[spec.Company],
                    spec.Deal is { } deal ? deals[deal] : null,
                    spec.Title,
                    null,
                    dueAt,
                    spec.Priority,
                    person
                )
            );

            if (spec.Done)
                await bus.InvokeAsync<TaskItemCompleted>(new CompleteTaskItem(created.TaskId, organizationId, person));
        }

        return TaskSpecs.Length;
    }

    private async Task<Guid?> UserByEmail(Guid organizationId, string prefix)
    {
        var user = await session
            .Query<UserProjection>()
            .Where(u => u.OrganizationId == organizationId && u.Email.StartsWith(prefix))
            .Select(u => u.Id)
            .ToListAsync();

        return user.Count > 0 ? user[0] : null;
    }
}
