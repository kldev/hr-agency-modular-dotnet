using HrAgencySystem.IntegrationTests.Candidates;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Workers.Events;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Workers;

/// <summary>
/// A workers' file opened from an application, end to end: the message crosses from Workers to
/// Recruitment, and both lists can then be filtered by it.
/// <para>
/// The application is written straight to the event store. Creating one over HTTP needs a company,
/// a job description and a published post, none of which this test is about.
/// </para>
/// </summary>
[Collection(IntegrationCollection.Name)]
public class WorkerFromRecruitmentTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanWorkers();
        await Cleaner.CleanCandidates();
        await Cleaner.CleanJobApplications();
    }

    [Fact]
    public async Task Registering_a_worker_from_an_application_marks_both_lists()
    {
        var organizationId = Guid.NewGuid();

        var registered = await CandidateWithApplicationAsync(organizationId, "anna@agency.test");
        var untouched = await CandidateWithApplicationAsync(organizationId, "piotr@agency.test");

        var worker = await RegisterFromAsync(organizationId, registered);

        await Eventually.AssertAsync(
            async () =>
            {
                var application = Assert.Single(await Applications(organizationId, true));
                Assert.Equal(registered.ApplicationId, application.Id);
                Assert.Equal(worker.WorkerId, application.WorkerId);

                var candidate = Assert.Single(await Candidates(organizationId, true));
                Assert.Equal(registered.CandidateId, candidate.Id);
                Assert.Equal(worker.WorkerId, candidate.WorkerId);
            },
            TimeSpan.FromSeconds(10)
        );

        var withoutApplication = Assert.Single(await Applications(organizationId, false));
        Assert.Equal(untouched.ApplicationId, withoutApplication.Id);
        Assert.Null(withoutApplication.WorkerId);

        var withoutCandidate = Assert.Single(await Candidates(organizationId, false));
        Assert.Equal(untouched.CandidateId, withoutCandidate.Id);

        Assert.Equal(2, (await Applications(organizationId, null)).Count);
        Assert.Equal(2, (await Candidates(organizationId, null)).Count);
    }

    private sealed record Source(Guid CandidateId, Guid ApplicationId);

    private async Task<Source> CandidateWithApplicationAsync(Guid organizationId, string email)
    {
        var candidate = await CandidateClient.Create(organizationId, Email: email);
        var applicationId = Guid.NewGuid();

        await using var session = Services.GetRequiredService<IDocumentStore>().LightweightSession();

        session.Events.StartStream<JobApplication>(
            applicationId,
            new JobApplicationCreated(
                applicationId,
                organizationId,
                Guid.NewGuid(),
                "Backend developer",
                CandidateSource.Direct,
                new CompanySnapshot(Guid.NewGuid(), "Client", ""),
                new CandidateInfo(
                    candidate.CandidateId,
                    email,
                    "+1 123 123 123",
                    "joe",
                    "smith"
                ),
                email,
                "+1 123 123 123",
                "joe",
                "smith",
                DateTimeOffset.UtcNow
            )
        );

        await session.SaveChangesAsync();

        // Both read models have to exist before the filter can be asked about them.
        await Eventually.AssertAsync(async () =>
        {
            Assert.Contains(
                await Applications(organizationId, null),
                application => application.Id == applicationId
            );
            Assert.Contains(
                await Candidates(organizationId, null),
                projection => projection.Id == candidate.CandidateId
            );
        });

        return new Source(candidate.CandidateId, applicationId);
    }

    private async Task<WorkerRegistered> RegisterFromAsync(Guid organizationId, Source source)
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.PostAsJsonAsync(
            "/api/workers",
            WorkerTestData.RegisterRequest(
                sourceCandidateId: source.CandidateId,
                sourceApplicationId: source.ApplicationId
            )
        );
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<WorkerRegistered>(OutputHelper);
        Assert.NotNull(created);
        Assert.Equal(source.ApplicationId, created.SourceApplicationId);

        return created;
    }

    private async Task<IReadOnlyList<JobApplicationProjection>> Applications(
        Guid organizationId,
        bool? registeredAsWorker
    ) => await Slice<JobApplicationProjection>(
        organizationId,
        "/api/recruitment/job-applications",
        registeredAsWorker
    );

    private async Task<IReadOnlyList<CandidateProjection>> Candidates(
        Guid organizationId,
        bool? registeredAsWorker
    ) => await Slice<CandidateProjection>(
        organizationId,
        CandidateTestClient.BaseUrl,
        registeredAsWorker
    );

    private async Task<IReadOnlyList<T>> Slice<T>(
        Guid organizationId,
        string url,
        bool? registeredAsWorker
    )
    {
        Client.WithOrganizationId(organizationId);

        var query = registeredAsWorker is null
            ? url
            : $"{url}?registeredAsWorker={registeredAsWorker.Value.ToString().ToLowerInvariant()}";

        var response = await Client.GetAsync(query);
        response.EnsureSuccessStatusCode();

        var slice = await response.ReadWithJson<SliceResponse<T>>();

        return slice?.Content ?? [];
    }
}
