using HrAgencySystem.Recruitment.Application.Candidate.Create;
using HrAgencySystem.Recruitment.Application.JobApplication.Create;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using NSubstitute;


namespace HrAgencySystem.UnitTests.JobPostings.Handlers;

public class ApplyToJobApplicationHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession =
        Substitute.For<IDocumentSession>();

    private readonly ICandidateResolver _candidateResolver =
        Substitute.For<ICandidateResolver>();

    private readonly IJobPostQueryRepository _jobPostQueryRepository =
        Substitute.For<IJobPostQueryRepository>();

    private readonly ICompanySnapshotRepository _companySnapshotRepository =
        Substitute.For<ICompanySnapshotRepository>();
    
    private readonly IClock _clock =
        Substitute.For<IClock>();

    private static readonly Guid JobPostId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid EventId = Guid.NewGuid();

    private static readonly DateTimeOffset Now =
        new(2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsJobApplicationCreated()
    {
        var candidate = CreateCandidate();
        var company = CreateCompany();

        var post = CreateJobPostInfo();

        SetupJobPost(post);
        SetupCandidate(candidate);
        SetupCompany(company);

        _clock.UtcNow.Returns(Now);
        
        var command = CreateValidCommand();

        var result = await Handle(command, clock: _clock);

        Assert.NotEqual(Guid.Empty, result.JobApplicationId);
        Assert.Equal(OrganizationId, result.OrganizationId);
        Assert.Equal(JobPostId, result.JobPostId);
        Assert.Equal("Senior .NET Developer", result.JobPostTitle);
        Assert.Equal(command.Source, result.Source);
        Assert.Equal(company, result.Company);
        Assert.Equal(candidate, result.CandidateInfo);
        Assert.Equal(command.Email, result.ApplicantEmail);
        Assert.Equal(command.Phone, result.ApplicantPhone);
        Assert.Equal(command.ToFullName(), result.FullName);
        
        Assert.Equal(Now, result.CreatedAt);

        await _jobPostQueryRepository
            .Received(1)
            .GetJobPostInfo(
                JobPostId,
                Arg.Any<CancellationToken>());

        await _candidateResolver
            .Received(1)
            .FindOrCreate(
                Arg.Is<CreateCandidate>(x =>
                    x.OrganizationId == OrganizationId &&
                    x.Email == command.Email &&
                    x.Source == command.Source &&
                    x.Phone == command.Phone &&
                    x.FirstName == command.FirstName &&
                    x.LastName == command.LastName &&
                    x.CompanyId == CompanyId),
                post,
                Arg.Any<CancellationToken>());

        await _companySnapshotRepository
            .Received(1)
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>());

        _documentSession.Events
            .Received(1)
            .StartStream<JobApplication>(
                result.JobApplicationId,
                Arg.Is<JobApplicationCreated>(
                    x => x.JobApplicationId == result.JobApplicationId));
    }

    [Theory]
    [InlineData(JobPostStatus.Draft)]
    [InlineData(JobPostStatus.Closed)]
    [InlineData(JobPostStatus.Archived)]
    public async Task Handle_WithJobPostNotPublished_ThrowsBusinessRuleException(
        JobPostStatus status)
    {
        var post = CreateJobPostInfo(status);

        SetupJobPost(post);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(CreateValidCommand()));

        Assert.Equal(
            "Applications are only allowed for published job posts.",
            exception.Message);

        await _jobPostQueryRepository
            .Received(1)
            .GetJobPostInfo(
                JobPostId,
                Arg.Any<CancellationToken>());

        AssertNoCandidateLookup();
        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            email: "invalid-email");

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Handle(command));

        Assert.NotEmpty(exception.Errors);

        AssertNoJobPostLookup();
        AssertNoCandidateLookup();
        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithInvalidPhone_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            phone: new string('1', 51));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Handle(command));

        Assert.NotEmpty(exception.Errors);

        AssertNoJobPostLookup();
        AssertNoCandidateLookup();
        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingCompany_ThrowsNotFoundException()
    {
        var post = CreateJobPostInfo();

        SetupJobPost(post);
        SetupCandidate(CreateCandidate());

        _companySnapshotRepository
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>())
            .Returns((CompanySnapshot?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => Handle(CreateValidCommand()));

        Assert.Equal(
            ICompanySnapshotRepository.NotFoundMessage,
            exception.Message);

        await _companySnapshotRepository
            .Received(1)
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithCandidateResolverReturningCandidate_StartsJobApplicationStream()
    {
        var post = CreateJobPostInfo();
        var candidate = CreateCandidate();
        var company = CreateCompany();

        SetupJobPost(post);
        SetupCandidate(candidate);
        SetupCompany(company);

        var result = await Handle(CreateValidCommand());

        _documentSession.Events
            .Received(1)
            .StartStream<JobApplication>(
                result.JobApplicationId,
                Arg.Is<JobApplicationCreated>(
                    x =>
                        x.JobApplicationId == result.JobApplicationId &&
                        x.OrganizationId == OrganizationId &&
                        x.JobPostId == JobPostId &&
                        x.JobPostTitle == "Senior .NET Developer" &&
                        x.CandidateInfo == candidate &&
                        x.Company == company));
    }

    private async Task<JobApplicationCreated> Handle(
        ApplyToJobApplication command,
        IClock? clock = null)
    {
        return await ApplyToJobApplicationHandler.Handle(
            command,
            _candidateResolver,
            _jobPostQueryRepository,
            _companySnapshotRepository,
            _documentSession,
            clock ?? TestClock,
            CancellationToken.None);
    }

    private void SetupJobPost(
        JobPostInfo post)
    {
        _jobPostQueryRepository
            .GetJobPostInfo(
                post.Id,
                Arg.Any<CancellationToken>())
            .Returns(post);
    }

    private void SetupCandidate(
        CandidateInfo candidate)
    {
        _candidateResolver
            .FindOrCreate(
                Arg.Any<CreateCandidate>(),
                Arg.Any<JobPostInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(candidate);
    }

    private void SetupCompany(
        CompanySnapshot company)
    {
        _companySnapshotRepository
            .GetCompanyAsync(
                CompanyId,
                Arg.Any<CancellationToken>())
            .Returns(company);
    }

    private void AssertNoJobPostLookup()
    {
        _jobPostQueryRepository
            .DidNotReceive()
            .GetJobPostInfo(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoCandidateLookup()
    {
        _candidateResolver
            .DidNotReceive()
            .FindOrCreate(
                Arg.Any<CreateCandidate>(),
                Arg.Any<JobPostInfo>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoCompanyLookup()
    {
        _companySnapshotRepository
            .DidNotReceive()
            .GetCompanyAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoStream()
    {
        _documentSession.Events
            .DidNotReceive()
            .StartStream<JobApplication>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    private static ApplyToJobApplication CreateValidCommand(
        string email = "john.doe@example.com",
        string phone = "+48 500 600 700",
        CandidateSource source = CandidateSource.RocketJobs,
        string? firstName = "John",
        string? lastName = "Doe")
    {
        return new ApplyToJobApplication(
            JobPostId,
            EventId,
            email,
            phone,
            source,
            firstName,
            lastName);
    }

    private static JobPostInfo CreateJobPostInfo(
        JobPostStatus status = JobPostStatus.Published)
    {
        return new JobPostInfo(
            JobPostId,
            OrganizationId,
            CompanyId,
            "Senior .NET Developer",
            status);
    }

    private static CompanySnapshot CreateCompany()
    {
        return new CompanySnapshot(
            CompanyId,
            "Company A",
            "TX-100-101");
    }

    private static CandidateInfo CreateCandidate()
    {
        return new CandidateInfo(
            Guid.NewGuid(),
            "john.doe@example.com",
            "+48 500 600 700",
            "John",
            "Doe");

    }
}
