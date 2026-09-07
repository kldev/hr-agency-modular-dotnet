using System.Net;
using HrAgencySystem.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Interviews;

[Collection(IntegrationCollection.Name)]
public sealed class InterviewGetTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();
    private readonly Guid OtherOrganizationId = Guid.NewGuid();

    private readonly Guid InterviewerId = Guid.NewGuid();
    private readonly Guid CreatedById = Guid.NewGuid();
    private readonly Guid JobApplicationId = Guid.NewGuid();

    private readonly DateTime TestDate = new(
        2026,
        9,
        10,
        10,
        0,
        0);

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanInterviews();
    }

    [Fact]
    public async Task ShouldReturnInterviewForCorrectOrganization()
    {
        var created = await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            InterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interview = await InterviewClient.GetAsync(
                OrganizationId,
                created.InterviewId);

            Assert.NotNull(interview);

            Assert.Equal(
                created.InterviewId,
                interview.Id);

            Assert.Equal(
                OrganizationId,
                interview.OrgId);

            Assert.Equal(
                JobApplicationId,
                interview.ApplicationId);

            Assert.Equal(
                InterviewerId,
                interview.InterviewerId);
        });
    }

    [Fact]
    public async Task ShouldNotReturnInterviewFromAnotherOrganization()
    {
        var created = await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            InterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interview = await InterviewClient.GetAsync(
                OtherOrganizationId,
                created.InterviewId);

            Assert.Null(interview);
        });
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownInterviewId()
    {
        var interview = await InterviewClient.GetAsync(
            OrganizationId,
            Guid.NewGuid());

        Assert.Null(interview);
    }

    [Fact]
    public async Task ShouldReturnProblemDetailsWhenInterviewBelongsToAnotherOrganization()
    {
        var created = await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            InterviewerId,
            CreatedById);

        Client.WithOrganizationId(OtherOrganizationId);
        
        await Eventually.AssertAsync(async () =>
        {
            var response = await Client.GetAsync("api/interviews/" + created.InterviewId);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);

            Assert.Equal(
                "application/json",
                response.Content.Headers.ContentType?.MediaType);

            var problem = await response.ReadWithJson<ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                (int)HttpStatusCode.NotFound,
                problem.Status);

            Assert.NotNull(problem.Title);
        });
    }
}