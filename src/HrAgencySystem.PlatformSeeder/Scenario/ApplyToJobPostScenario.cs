using Bogus;
using HrAgencySystem.Recruitment.Application.JobApplications.Create;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.JobPostings;
using Marten;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

public sealed class ApplyToJobPostScenario(
    IMessageBus bus,
    IQuerySession session)
{
    private const int ApplicationsPerCandidate = 5;
    public async Task Execute(int count = 500)
    {
        var jobs = await GetJobs();

        if (jobs.Count == 0)
            return;

        await CreateShowcaseCandidates(jobs);
        await CreateRandomCandidates(count, jobs);
    }
    
    public async Task ExecuteShowcase()
    {
        var jobs = await GetJobs();

        if (jobs.Count == 0)
            return;

        await CreateShowcaseCandidates(jobs);
    }

    private async Task CreateShowcaseCandidates(IReadOnlyList<Guid> jobs)
    {
        var candidates = new[]
        {
            new CandidateSeed(
                "Fox",
                "Mulder",
                "fox.mulder@fbi.gov",
                "+1 202-555-0101"),

            new CandidateSeed(
                "Dana",
                "Scully",
                "dana.scully@fbi.gov",
                "+1 202-555-0102"),

            // Friends

            new CandidateSeed(
                "Rachel",
                "Green",
                "rachel.green@friends.example",
                "+1 212-555-0101"),

            new CandidateSeed(
                "Ross",
                "Geller",
                "ross.geller@friends.example",
                "+1 212-555-0102"),

            new CandidateSeed(
                "Monica",
                "Geller",
                "monica.geller@friends.example",
                "+1 212-555-0103"),

            new CandidateSeed(
                "Chandler",
                "Bing",
                "chandler.bing@friends.example",
                "+1 212-555-0104"),

            new CandidateSeed(
                "Joey",
                "Tribbiani",
                "joey.tribbiani@friends.example",
                "+1 212-555-0105"),

            new CandidateSeed(
                "Phoebe",
                "Buffay",
                "phoebe.buffay@friends.example",
                "+1 212-555-0106"),
        };

        var commands = candidates
            .SelectMany(candidate => CreateApplications(candidate, jobs));

        await ExecuteInSequence(commands);
    }

    private async Task CreateRandomCandidates(
        int count,
        IReadOnlyList<Guid> jobs)
    {
        var faker = new Faker();

        var commands = Enumerable
            .Range(0, count)
            .SelectMany(_ => CreateApplications(faker, jobs));

        await ExecuteInSequence(commands);
    }

    private static IEnumerable<ApplyToJobApplication> CreateApplications(
        CandidateSeed candidate,
        IReadOnlyList<Guid> jobs)
    {
        for (var i = 0; i < ApplicationsPerCandidate; i++)
        {
            yield return new ApplyToJobApplication(
                JobPostId: jobs[Random.Shared.Next(jobs.Count)],
                Email: candidate.Email,
                Phone: candidate.Phone,
                Source: RandomSource(),
                FirstName: candidate.FirstName,
                LastName: candidate.LastName);
        }
    }

    private static IEnumerable<ApplyToJobApplication> CreateApplications(
        Faker faker,
        IReadOnlyList<Guid> jobs)
    {
        var firstName = faker.Person.FirstName;
        var lastName = faker.Person.LastName;
        var index = faker.UniqueIndex;

        var email = faker.Internet.Email(
            firstName,
            lastName + Random.Shared.Next(1000, 99999),
            uniqueSuffix: index.ToString());

        var phone = faker.Phone.PhoneNumber();

        for (var i = 0; i < ApplicationsPerCandidate; i++)
        {
            yield return new ApplyToJobApplication(
                JobPostId: jobs[Random.Shared.Next(jobs.Count)], 
                Email: email,
                Phone: phone,
                Source: RandomSource(),
                FirstName: firstName,
                LastName: lastName);
        }
    }

    private async Task ExecuteInSequence(
        IEnumerable<ApplyToJobApplication> commands)
    {
        foreach (var command in commands)
        {
            await ApplyToJobPost(command);
        }
    }

    private async Task ApplyToJobPost(ApplyToJobApplication command)
    {
        try
        {
            await bus.InvokeAsync<JobApplicationCreated>(command);
        }
        catch
        {
            // Ignore invalid applications during seeding.
        }
    }

    private async Task<IReadOnlyList<Guid>> GetJobs()
    {
        return await session
            .Query<JobPostCreated>()
            .Select(x => x.JobPostId)
            .ToListAsync();
    }

    private static CandidateSource RandomSource()
    {
        var values = Enum.GetValues<CandidateSource>();

        return values[Random.Shared.Next(values.Length)];
    }

    private sealed record CandidateSeed(
        string FirstName,
        string LastName,
        string Email,
        string Phone);
}