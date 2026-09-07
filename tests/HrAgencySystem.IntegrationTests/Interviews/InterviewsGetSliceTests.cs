using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Domain.Interviews;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Interviews;

[Collection(IntegrationCollection.Name)]
public sealed class InterviewsGetSliceTests(
    IntegrationEnvironment environment,
    ITestOutputHelper output)
    : BaseIntegrationTest(environment, output)
{
    private readonly Guid OrganizationId = Guid.NewGuid();

    private readonly Guid InterviewerId = Guid.NewGuid();
    private readonly Guid SecondInterviewerId = Guid.NewGuid();

    private readonly Guid CreatedById = Guid.NewGuid();
    private readonly Guid SecondCreatedById = Guid.NewGuid();

    private readonly Guid JobApplicationId = Guid.NewGuid();
    private readonly Guid SecondJobApplicationId = Guid.NewGuid();

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
    public async Task ShouldReturnAllInterviews()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            InterviewerId,
            CreatedById);
        
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId);

            Assert.Single(interviews.Content);
            Assert.Equal(
                InterviewerId,
                interviews.Content[0].InterviewerId);

            Assert.Equal(
                TestDate.ToUniversalTime(),
                interviews.Content[0].ScheduleAt);
        });
    }

    [Fact]
    public async Task ShouldReturnInterviewsForCandidate()
    {
        var result = await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            InterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                candidateId: result.CandidateId);

            Assert.Single(interviews.Content);

            Assert.Equal(
                InterviewerId,
                interviews.Content[0].InterviewerId);

            Assert.Equal(
                result.CandidateId,
                interviews.Content[0].CandidateId);
        });
    }

    [Fact]
    public async Task ShouldFilterByJobApplication()
    {
        var secondJobApplicationId = Guid.NewGuid();

        await InterviewClient.ScheduleAsync(
            OrganizationId,
            secondJobApplicationId,
            TestDate.AddHours(2),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                jobApplicationId: secondJobApplicationId);

            Assert.Single(interviews.Content);

            Assert.Equal(
                secondJobApplicationId,
                interviews.Content[0].ApplicationId);
        });
    }

    [Fact]
    public async Task ShouldFilterByInterviewer()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddHours(2),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                interviewerId: SecondInterviewerId);

            Assert.Single(interviews.Content);

            Assert.Equal(
                SecondInterviewerId,
                interviews.Content[0].InterviewerId);
        });
    }

    [Fact]
    public async Task ShouldFilterByCreatedByUser()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddHours(2),
            SecondInterviewerId,
            SecondCreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                createdByUserId: SecondCreatedById);

            Assert.Single(interviews.Content);

            Assert.Equal(
                SecondCreatedById,
                interviews.Content[0].CreatedByUserId);
        });
    }

    [Fact]
    public async Task ShouldFilterByStatus()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            SecondInterviewerId,
            CreatedById);
        
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                status: InterviewStatus.Planned);

            Assert.Single(interviews.Content);

            Assert.Equal(
                InterviewStatus.Planned,
                interviews.Content[0].Status);
        });
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenStatusDoesNotMatch()
    {
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                status: InterviewStatus.Canceled);

            Assert.Empty(interviews.Content);
        });
    }

    [Fact]
    public async Task ShouldFilterByFromDate()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddDays(5),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                fromDate: DateOnly.FromDateTime(TestDate.AddDays(1)));

            Assert.Single(interviews.Content);

            Assert.Equal(
                SecondInterviewerId,
                interviews.Content[0].InterviewerId);
        });
    }

    [Fact]
    public async Task ShouldFilterByToDate()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddSeconds(-1),
            InterviewerId,
            CreatedById);
        
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddDays(5),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                toDate: DateOnly.FromDateTime(TestDate));

            Assert.Single(interviews.Content);

            Assert.Equal(
                InterviewerId,
                interviews.Content[0].InterviewerId);
        });
    }

    [Fact]
    public async Task ShouldFilterByDateRange()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddDays(-5),
            SecondInterviewerId,
            CreatedById);
        

        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddDays(1),
            SecondInterviewerId,
            CreatedById);
        
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddDays(5),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                fromDate: DateOnly.FromDateTime(TestDate.AddDays(-1)),
                toDate: DateOnly.FromDateTime(TestDate.AddDays(2)));
            
            Assert.Single(interviews.Content);

            Assert.Equal(
                SecondInterviewerId,
                interviews.Content[0].InterviewerId);
        });
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenDateRangeDoesNotContainInterview()
    {
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                fromDate: DateOnly.FromDateTime(TestDate.AddDays(1)),
                toDate: DateOnly.FromDateTime(TestDate.AddDays(2)));

            Assert.Empty(interviews.Content);
        });
    }

    [Fact]
    public async Task ShouldApplyMultipleFilters()
    {
        var created = await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddHours(2),
            SecondInterviewerId,
            SecondCreatedById);
        
        Assert.Equal(JobApplicationId, created.JobApplicationId);
        Assert.Equal(SecondInterviewerId, created.Interviewer.Id);
        Assert.Equal(SecondCreatedById, created.Author.Id);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                interviewerId: SecondInterviewerId,
                createdByUserId: SecondCreatedById,
                status: InterviewStatus.Planned);

            Assert.Single(interviews.Content);

            var interview = interviews.Content[0];

            Assert.Equal(
                SecondInterviewerId,
                interview.InterviewerId);

            Assert.Equal(
                SecondCreatedById,
                interview.CreatedByUserId);

            Assert.Equal(
                InterviewStatus.Planned,
                interview.Status);
        });
    }

    [Fact]
    public async Task ShouldReturnRequestedPage()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddHours(1),
            SecondInterviewerId,
            CreatedById);

        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddHours(2),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var firstPage = await InterviewClient.GetSliceAsync(
                OrganizationId,
                page: 1,
                pageSize: 1);

            var secondPage = await InterviewClient.GetSliceAsync(
                OrganizationId,
                page: 2,
                pageSize: 1);

            Assert.Single(firstPage.Content);
            Assert.Single(secondPage.Content);

            Assert.NotEqual(
                firstPage.Content[0].Id,
                secondPage.Content[0].Id);
        });
    }

    [Fact]
    public async Task ShouldReturnEmptyPageWhenPageIsAfterLastPage()
    {
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                page: 2,
                pageSize: 100);

            Assert.Empty(interviews.Content);
        });
    }

    [Fact]
    public async Task ShouldRespectPageSize()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddHours(1),
            SecondInterviewerId,
            CreatedById);

        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate.AddHours(2),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                page: 1,
                pageSize: 2);

            Assert.Equal(2, interviews.Content.Count);
        });
    }

    [Fact]
    public async Task ShouldReturnOnlyMatchingJobApplicationWhenMultipleApplicationsExist()
    {
        var first = await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            InterviewerId,
            CreatedById);

        var second = await InterviewClient.ScheduleAsync(
            OrganizationId,
            SecondJobApplicationId,
            TestDate.AddHours(1),
            SecondInterviewerId,
            CreatedById);

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                jobApplicationId: SecondJobApplicationId);

            Assert.Single(interviews.Content);

            Assert.Equal(
                second.JobApplicationId,
                interviews.Content[0].ApplicationId);

            Assert.NotEqual(
                first.JobApplicationId,
                interviews.Content[0].ApplicationId);
        });
    }

    [Fact]
    public async Task ShouldReturnOnlyInterviewsFromRequestedOrganization()
    {
        var otherOrganizationId = Guid.NewGuid();
        
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            JobApplicationId,
            TestDate,
            InterviewerId,
            CreatedById);

        await InterviewClient.ScheduleAsync(
            otherOrganizationId,
            Guid.NewGuid(),
            TestDate,
            Guid.NewGuid(),
            Guid.NewGuid());

        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId);

            Assert.Single(interviews.Content);

            Assert.Equal(
                OrganizationId,
                interviews.Content[0].OrgId);
        });
    }

    [Fact]
    public async Task ShouldReturnInterviewWhenFromDateEqualsInterviewDate()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            Guid.NewGuid(),
            TestDate.AddSeconds(1),
            InterviewerId,
            Guid.NewGuid());
        
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                fromDate: DateOnly.FromDateTime(TestDate));

            Assert.Single(interviews.Content);

            Assert.Equal(
                InterviewerId,
                interviews.Content[0].InterviewerId);
        });
    }

    [Fact]
    public async Task ShouldReturnInterviewWhenToDateEqualsInterviewDate()
    {
        await InterviewClient.ScheduleAsync(
            OrganizationId,
            Guid.NewGuid(),
            TestDate,
            InterviewerId,
            Guid.NewGuid());
        
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                toDate: DateOnly.FromDateTime(TestDate));

            Assert.Single(interviews.Content);

            Assert.Equal(
                InterviewerId,
                interviews.Content[0].InterviewerId);
        });
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenInterviewerDoesNotExist()
    {
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                interviewerId: Guid.NewGuid());

            Assert.Empty(interviews.Content);
        });
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenJobApplicationDoesNotExist()
    {
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                jobApplicationId: Guid.NewGuid());

            Assert.Empty(interviews.Content);
        });
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenCandidateDoesNotExist()
    {
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                candidateId: Guid.NewGuid());

            Assert.Empty(interviews.Content);
        });
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenCreatedByUserDoesNotExist()
    {
        await Eventually.AssertAsync(async () =>
        {
            var interviews = await InterviewClient.GetSliceAsync(
                OrganizationId,
                createdByUserId: Guid.NewGuid());

            Assert.Empty(interviews.Content);
        });
    }
}