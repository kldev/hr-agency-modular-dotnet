using HrAgencySystem.Company.Projections;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;
using HrAgencySystem.IntegrationTests.JobDescriptions;
using HrAgencySystem.IntegrationTests.JobPosts;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Companies;

/// <summary>
/// A company counts the posts published for it, and that count is fed by an integration event from
/// <c>Recruitment</c> which appends to the company's own stream.
/// </summary>
[Collection(IntegrationCollection.Name)]
public sealed class CompanyJobPostCountTests(IntegrationEnvironment env, ITestOutputHelper output)
    : BaseIntegrationTest(env, output)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanJobDescriptions();
        await Cleaner.CleanCompany();
    }

    /// <summary>
    /// Posts for one client go up in a batch, and every one of them writes onto that client's
    /// stream. Appending without a lock lets two of those transactions work out the same next
    /// stream version; one dies on the primary key, its message dead-letters, and the company
    /// quietly under-reports how many posts it has. Nothing in the UI would ever say so.
    /// </summary>
    [Fact]
    public async Task Several_posts_published_at_once_are_all_counted_on_the_company()
    {
        const int posts = 4;

        var organizationId = Guid.NewGuid();
        Client.WithOrganizationId(organizationId);

        var company = await CompanyClient.CreateAsync(organizationId);

        JobDescriptionClient.WithOrganizationId(organizationId);
        var description = await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest(company.CompanyId)
        );

        JobPostingClient.WithOrganizationId(organizationId);

        // The job post reads its company off the description through a snapshot port, faked here.
        // Pinned so all four posts land on one company stream - which is the whole point.
        FakeJobDescriptionSnapshot.PinnedCompanyId = company.CompanyId;

        try
        {
            await Task.WhenAll(
                Enumerable
                    .Range(0, posts)
                    .Select(_ =>
                        JobPostingClient.CreateAsync(
                            JobPostingTestData.CreateRequest(description.JobDescriptionId)
                        )
                    )
            );
        }
        finally
        {
            FakeJobDescriptionSnapshot.PinnedCompanyId = null;
        }

        await Eventually.AssertAsync(async () =>
        {
            Client.WithOrganizationId(organizationId);

            var response = await Client.GetAsync($"/api/companies/{company.CompanyId}");
            response.EnsureSuccessStatusCode();

            var projection = await response.ReadWithJson<CompanyProjection>();

            Assert.NotNull(projection);
            Assert.Equal(posts, projection.JobsPostCount);
        });
    }
}
