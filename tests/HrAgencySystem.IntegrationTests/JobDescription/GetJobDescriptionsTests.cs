using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Projections;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.JobDescription;

[Collection(IntegrationCollection.Name)]
public sealed class GetJobDescriptionsTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    private Guid CompanyId { get; set; }
    private Guid OtherCompanyId { get; set; }

    private Guid RecruiterId { get; set; }
    private Guid OtherRecruiterId { get; set; }

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanJobDescriptions();

        await SetupData();
    }

    private async Task SetupData()
    {
        CompanyId = Guid.NewGuid();
        OtherCompanyId = Guid.NewGuid();

        RecruiterId = Guid.NewGuid();
        OtherRecruiterId = Guid.NewGuid();

        JobDescriptionClient.WithOrganizationId(OrganizationId);

        await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest(
                companyId: CompanyId,
                recruiterId: RecruiterId));

        await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest(
                companyId: CompanyId,
                recruiterId: RecruiterId) with
            {
                Title = "Backend Developer",
                Summary = "Backend Developer position"
            });

        await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest(
                companyId: OtherCompanyId,
                recruiterId: OtherRecruiterId) with
            {
                Title = "Frontend Developer",
                Summary = "Frontend Developer position"
            });

        JobDescriptionClient.WithOrganizationId(OtherOrganizationId);

        await JobDescriptionClient.CreateAsync(
            JobDescriptionTestData.CreateRequest(
                companyId: Guid.NewGuid(),
                recruiterId: Guid.NewGuid()) with
            {
                Title = "Other Organization Developer"
            });

        JobDescriptionClient.WithOrganizationId(OrganizationId);
    }

    [Fact]
    public async Task ShouldGetJobDescriptionsForAllScenarios()
    {
        await ShouldGetJobDescriptionsFromOrganization();
        await ShouldFilterBySearch();
        await ShouldFilterByCompany();
        await ShouldFilterByRecruiter();
        await ShouldFilterByStatus();
        await ShouldFilterByMultipleStatuses();
        await ShouldPaginate();
        await ShouldNotGetJobDescriptionsFromOtherOrganization();
    }

    private async Task ShouldGetJobDescriptionsFromOrganization()
    {
        await Eventually.AssertAsync(async () =>
        {
            var result = await GetJobDescriptionsAsync();
            Assert.Equal(3, result.Count);

            Assert.Contains(result, x => x.Title == "Senior .NET Developer");
            Assert.Contains(result, x => x.Title == "Backend Developer");
            Assert.Contains(result, x => x.Title == "Frontend Developer");

            Assert.DoesNotContain(
                result,
                x => x.Title == "Other Organization Developer");
        });
    }

    private async Task ShouldFilterBySearch()
    {
        await Eventually.AssertAsync(
            async () =>
            {
                var result = await GetJobDescriptionsAsync(search: "backend");
                Assert.Single(result);
                Assert.Equal("Backend Developer", result[0].Title);
            });
    }

    private async Task ShouldFilterByCompany()
    {
        await Eventually.AssertAsync(
            async () =>
            {
                var result = await GetJobDescriptionsAsync(
                    companyId: CompanyId);
                Assert.Equal(2, result.Count);

                Assert.All(
                    result,
                    x => Assert.Equal(CompanyId, x.CompanyId));
            });
    }

    private async Task ShouldFilterByRecruiter()
    {
        await Eventually.AssertAsync(
            async () =>
            {
                var result =  await GetJobDescriptionsAsync(
                    recruiterId: RecruiterId);
                Assert.Equal(2, result.Count);

                Assert.All(
                    result,
                    x => Assert.Equal(RecruiterId, x.RecruiterId));
            });
    }

    private async Task ShouldFilterByStatus()
    {
        await Eventually.AssertAsync(
            async () =>
            {
                var result = await GetJobDescriptionsAsync(
                    status: [JobDescriptionStatus.Draft]);
                
                Assert.All(
                    result,
                    x => Assert.Equal(JobDescriptionStatus.Draft, x.Status));
            });
        
    }

    private async Task ShouldFilterByMultipleStatuses()
    {
        await Eventually.AssertAsync(
            async () =>
            {
                var result = await GetJobDescriptionsAsync(
                    status:
                    [
                        JobDescriptionStatus.Draft,
                        JobDescriptionStatus.Open
                    ]);
                
                Assert.All(
                    result,
                    x => Assert.Contains(
                        x.Status,
                        new[]
                        {
                            JobDescriptionStatus.Draft,
                            JobDescriptionStatus.Open
                        }));
            });

        
    }

    private async Task ShouldPaginate()
    {
        IReadOnlyList<JobDescriptionProjection> firstPage = null!;
        await Eventually.AssertAsync(async () =>
        {
            firstPage = await GetJobDescriptionsAsync(
                page: 1,
                pageSize: 2);

            Assert.Equal(2, firstPage.Count);
        });

        await Eventually.AssertAsync(async () =>
        {
            var secondPage = await GetJobDescriptionsAsync(
                page: 2,
                pageSize: 2);
            Assert.Single(secondPage);
            Assert.DoesNotContain(
                firstPage,
                first => secondPage.Any(second => second.Id == first.Id));
        });
    }

    private async Task ShouldNotGetJobDescriptionsFromOtherOrganization()
    {
        JobDescriptionClient.WithOrganizationId(OtherOrganizationId);

        await Eventually.AssertAsync(
            async () =>
            {
                var result = await GetJobDescriptionsAsync();
                Assert.Single(result);
                Assert.Equal(
                    "Other Organization Developer",
                    result[0].Title);
            });

        JobDescriptionClient.WithOrganizationId(OrganizationId);
    }

    private async Task<IReadOnlyList<JobDescriptionProjection>> GetJobDescriptionsAsync(
        string search = "",
        Guid? companyId = null,
        Guid? recruiterId = null,
        JobDescriptionStatus[]? status = null,
        int page = 1,
        int pageSize = 100)
    {
        var statusUrl = status!=null & status is { Length: > 0 } ? "&status=" + string.Join("&status=", status!) : "";
        
        var url = $"/api/job-description?search={search}&companyId={companyId ?? Guid.Empty}&recruiterId={recruiterId ?? Guid.Empty}{statusUrl}" ;
        url += $"&page={page}&pageSize={pageSize}";

        OutputHelper.WriteLine("Url: " +url);
        
        var result = await JobDescriptionClient.GetSlice(url);

        OutputHelper.WriteLine(
            $"Returned {result.Content.Count} job descriptions.");

        return result.Content;
    }
}
