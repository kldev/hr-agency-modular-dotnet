using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.JobPostings.Handlers;

public class CreateJobPostHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession =
        Substitute.For<IDocumentSession>();

    private readonly IOrganizationChecker _checker =
        Substitute.For<IOrganizationChecker>();

    private readonly IUserSnapshotRepository _userSnapshotRepository =
        Substitute.For<IUserSnapshotRepository>();

    private readonly ICompanySnapshotRepository _companySnapshotRepository =
        Substitute.For<ICompanySnapshotRepository>();

    private readonly IJobDescriptionSnapshotRepository _jobDescriptionSnapshotRepository =
        Substitute.For<IJobDescriptionSnapshotRepository>();

    private static readonly Guid RecruiterId = Guid.NewGuid();
    private static readonly Guid CreatedById = Guid.NewGuid();

    private static UserSnapshot Recruiter { get; } =
        new(
            RecruiterId,
            "Alice",
            "Wells",
            "alice-wells@hr-agency.com");

    private static UserSnapshot CreatedBy { get; } =
        new(
            CreatedById,
            "Bob",
            "Smith",
            "bob-smith@hr-agency.com");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsJobPostCreated()
    {
        var organizationId = Guid.NewGuid();
        var jobDescriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var now = new DateTimeOffset(
            2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        var command = CreateValidCommand(
            organizationId: organizationId,
            jobDescriptionId: jobDescriptionId,
            recruiterId: RecruiterId,
            createdBy: CreatedById);

        var jobDescription = new JobDescriptionSnapshot(
            jobDescriptionId,
            "Test",
            companyId);

        var company = new CompanySnapshot(
            companyId,
            "Company A",
            "TX-100-101");

        _checker
            .GetSlug(
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns("company-a");

        _userSnapshotRepository
            .GetUserAsync(
                RecruiterId,
                Arg.Any<CancellationToken>())
            .Returns(Recruiter);

        _userSnapshotRepository
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>())
            .Returns(CreatedBy);

        _jobDescriptionSnapshotRepository
            .GetAsync(
                jobDescriptionId,
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns(jobDescription);

        _companySnapshotRepository
            .GetCompanyAsync(
                companyId,
                Arg.Any<CancellationToken>())
            .Returns(company);

        var result = await Handle(
            command,
            new FixedClock(now));

        Assert.NotEqual(Guid.Empty, result.JobPostId);
        Assert.Equal(jobDescriptionId, result.JobDescriptionId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(companyId, result.CompanyId);

        Assert.Equal(
            "Senior .NET Developer",
            result.Title);

        Assert.Equal(
            "Senior developer position",
            result.Summary);

        Assert.Equal(
            "We are looking for an experienced .NET developer.",
            result.Description);

        Assert.Equal(
            [
                "Design and develop backend services.",
                "Review code."
            ],
            result.Responsibilities);

        Assert.Equal(
            [
                "5+ years of .NET experience.",
                "Experience with PostgreSQL."
            ],
            result.Requirements);

        Assert.Equal(
            [
                "C#",
                "ASP.NET Core"
            ],
            result.Skills);

        Assert.Equal(
            "Opole",
            result.Location);

        Assert.Equal(
            "PL",
            result.CountryCode);
        
        Assert.Equal(
            "PL",
            result.LanguageCode);

        Assert.Equal(
            EmploymentType.FullTime,
            result.EmploymentType);

        Assert.Equal(
            WorkMode.Hybrid,
            result.WorkMode);

        Assert.Equal(
            CurrencyCode.PLN,
            result.CurrencyCode);

        Assert.Equal(
            15000m,
            result.SalaryMin);

        Assert.Equal(
            22000m,
            result.SalaryMax);

        Assert.Equal(
            RecruiterId,
            result.Recruiter.Id);

        Assert.Equal(
            CreatedById,
            result.CreatedBy.Id);

        Assert.Equal(
            companyId,
            result.Company.Id);

        Assert.Equal(
            "company-a",
            result.OrgSlug);

        Assert.Equal(
            now,
            result.CreatedAt);

        Assert.NotEmpty(result.PostingSlug);

        await _checker
            .Received(1)
            .GetSlug(
                organizationId,
                Arg.Any<CancellationToken>());

        await _userSnapshotRepository
            .Received(1)
            .GetUserAsync(
                RecruiterId,
                Arg.Any<CancellationToken>());

        await _userSnapshotRepository
            .Received(1)
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>());

        await _jobDescriptionSnapshotRepository
            .Received(1)
            .GetAsync(
                jobDescriptionId,
                organizationId,
                Arg.Any<CancellationToken>());

        await _companySnapshotRepository
            .Received(1)
            .GetCompanyAsync(
                companyId,
                Arg.Any<CancellationToken>());

        _documentSession.Events
            .Received(1)
            .StartStream<JobPost>(
                result.JobPostId,
                Arg.Is<JobPostCreated>(
                    x => x.JobPostId == result.JobPostId));
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            title: "");

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Handle(command));

        Assert.NotEmpty(exception.Errors);

        await AssertNoOrganizationCheck();
        AssertNoUserLookup();
        AssertNoJobDescriptionLookup();
        AssertNoCompanyLookup();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingOrganization_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();

        _checker
            .GetSlug(
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(
                CreateValidCommand(
                    organizationId: organizationId)));

        Assert.Equal(
            OrganizationId.OrganizationCheckMessage,
            exception.Message);

        await _checker
            .Received(1)
            .GetSlug(
                organizationId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingRecruiter_ThrowsBusinessRuleException()
    {
        var recruiterId = Guid.NewGuid();

        SetupOrganization();

        _userSnapshotRepository
            .GetUserAsync(
                recruiterId,
                Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(
                CreateValidCommand(
                    recruiterId: recruiterId)));

        Assert.Equal(
            IUserSnapshotRepository.NotFoundMessage,
            exception.Message);

        await _userSnapshotRepository
            .Received(1)
            .GetUserAsync(
                recruiterId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingCreatedByUser_ThrowsBusinessRuleException()
    {
        var createdById = Guid.NewGuid();

        SetupOrganization();
        SetupRecruiter();

        _userSnapshotRepository
            .GetUserAsync(
                createdById,
                Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(
                CreateValidCommand(
                    createdBy: createdById)));

        Assert.Equal(
            IUserSnapshotRepository.NotFoundMessage,
            exception.Message);

        await _userSnapshotRepository
            .Received(1)
            .GetUserAsync(
                createdById,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingJobDescription_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();
        var jobDescriptionId = Guid.NewGuid();

        SetupOrganization();
        SetupRecruiter();
        SetupCreatedBy();

        _jobDescriptionSnapshotRepository
            .GetAsync(
                jobDescriptionId,
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns((JobDescriptionSnapshot?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(
                CreateValidCommand(
                    organizationId: organizationId,
                    jobDescriptionId: jobDescriptionId)));

        Assert.Equal(
            IJobDescriptionSnapshotRepository.NotFoundMessage,
            exception.Message);

        await _jobDescriptionSnapshotRepository
            .Received(1)
            .GetAsync(
                jobDescriptionId,
                organizationId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingCompany_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();
        var jobDescriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        SetupOrganization();
        SetupRecruiter();
        SetupCreatedBy();

        _jobDescriptionSnapshotRepository
            .GetAsync(
                jobDescriptionId,
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns(
                new JobDescriptionSnapshot(
                    jobDescriptionId,
                    "TEST",
                    companyId));

        _companySnapshotRepository
            .GetCompanyAsync(
                companyId,
                Arg.Any<CancellationToken>())
            .Returns((CompanySnapshot?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => Handle(
                CreateValidCommand(
                    organizationId: organizationId,
                    jobDescriptionId: jobDescriptionId)));

        Assert.Equal(
            ICompanySnapshotRepository.NotFoundMessage,
            exception.Message);

        await _companySnapshotRepository
            .Received(1)
            .GetCompanyAsync(
                companyId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    private async Task<JobPostCreated> Handle(
        CreateJobPost command,
        IClock? clock = null)
    {
        return await CreateJobPostHandler.Handle(
            command,
            _documentSession,
            clock ?? TestClock,
            _checker,
            _userSnapshotRepository,
            _companySnapshotRepository,
            _jobDescriptionSnapshotRepository,
            CancellationToken.None);
    }

    private void SetupOrganization(
        string slug = "company-a")
    {
        _checker
            .GetSlug(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(slug);
    }

    private void SetupRecruiter()
    {
        _userSnapshotRepository
            .GetUserAsync(
                RecruiterId,
                Arg.Any<CancellationToken>())
            .Returns(Recruiter);
    }

    private void SetupCreatedBy()
    {
        _userSnapshotRepository
            .GetUserAsync(
                CreatedById,
                Arg.Any<CancellationToken>())
            .Returns(CreatedBy);
    }

    private async Task AssertNoOrganizationCheck()
    {
        await _checker
            .DidNotReceive()
            .GetSlug(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoUserLookup()
    {
        _userSnapshotRepository
            .DidNotReceive()
            .GetUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoJobDescriptionLookup()
    {
        _jobDescriptionSnapshotRepository
            .DidNotReceive()
            .GetAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
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
            .StartStream<JobPost>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    private static CreateJobPost CreateValidCommand(
        Guid? organizationId = null,
        Guid? jobDescriptionId = null,
        string title = "Senior .NET Developer",
        string? summary = "Senior developer position",
        string description =
            "We are looking for an experienced .NET developer.",
        IReadOnlyList<string>? responsibilities = null,
        IReadOnlyList<string>? requirements = null,
        IReadOnlyList<string>? skills = null,
        string location = "Opole",
        string countryCode = "PL",
        string languageCode = "pl",
        EmploymentType employmentType = EmploymentType.FullTime,
        WorkMode workMode = WorkMode.Hybrid,
        CurrencyCode currencyCode = CurrencyCode.PLN,
        decimal salaryMin = 15000m,
        decimal salaryMax = 22000m,
        Guid? recruiterId = null,
        Guid? createdBy = null)
    {
        return new CreateJobPost(
            jobDescriptionId ?? Guid.NewGuid(),
            organizationId ?? Guid.NewGuid(),
            title,
            summary,
            description,
            responsibilities ??
            [
                "Design and develop backend services.",
                "Review code."
            ],
            requirements ??
            [
                "5+ years of .NET experience.",
                "Experience with PostgreSQL."
            ],
            skills ??
            [
                "C#",
                "ASP.NET Core"
            ],
            location,
            countryCode,
            languageCode,
            employmentType,
            workMode,
            currencyCode,
            salaryMin,
            salaryMax,
            recruiterId ?? RecruiterId,
            createdBy ?? CreatedById);
    }
}