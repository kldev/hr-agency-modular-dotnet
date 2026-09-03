using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Application.Handlers;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using NSubstitute;

namespace HrAgencySystem.UnitTests.JobDescription.Handlers;

public class CreateJobDescriptionHandlerTests : BaseTest
{
    private readonly IDocumentSession _documentSession =
            Substitute.For<IDocumentSession>();
    
    private readonly IOrganizationChecker _checker =
        Substitute.For<IOrganizationChecker>();

    private readonly IUserSnapshotRepository _snapshotRepository =
        Substitute.For<IUserSnapshotRepository>();

    private readonly ICompanySnapshotRepository _companySnapshot =
        Substitute.For<ICompanySnapshotRepository>();

    private static readonly Guid RecruiterId = Guid.NewGuid();

    private static UserSnapshot Recruiter { get; } =
        new(
            RecruiterId,
            "Alice",
            "Wells",
            "alice-wells@hr-agency.com");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsJobDescriptionCreated()
    {
        var organizationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var now = new DateTimeOffset(
            2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        var command = new CreateJobDescription(
            organizationId,
            companyId,
            "  Senior .NET Developer  ",
            "  Senior developer position  ",
            "  We are looking for an experienced .NET developer.  ",
            [
                "  Design and develop backend services.  ",
                "  Review code.  "
            ],
            [
                "  5+ years of .NET experience.  ",
                "  Experience with PostgreSQL.  "
            ],
            [
                "  C#  ",
                "  ASP.NET Core  "
            ],
            "  Opole  ",
            "pl",
            EmploymentType.FullTime,
            WorkMode.Hybrid,
            CurrencyCode.PLN,
            15000m,
            22000m,
            RecruiterId,
            RecruiterId);

        _checker
            .Exists(organizationId, Arg.Any<CancellationToken>())
            .Returns(true);

        _snapshotRepository
            .GetUserAsync(RecruiterId, Arg.Any<CancellationToken>())
            .Returns(Recruiter);

        _companySnapshot.GetCompanyAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new CompanySnapshot(companyId, "Company A", "TX-100-101"));

        var result = await Handle(
            command,
            new FixedClock(now));

        Assert.NotEqual(Guid.Empty, result.JobDescriptionId);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal("Senior .NET Developer", result.Title);
        Assert.Equal("Senior developer position", result.Summary);
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

        Assert.Equal("Opole", result.Location);
        Assert.Equal("PL", result.CountryCode);
        Assert.Equal(EmploymentType.FullTime, result.EmploymentType);
        Assert.Equal(WorkMode.Hybrid, result.WorkMode);
        Assert.Equal(CurrencyCode.PLN, result.CurrencyCode);
        Assert.Equal(15000m, result.SalaryMin);
        Assert.Equal(22000m, result.SalaryMax);
        Assert.Equal(RecruiterId, result.Recruiter.Id);
        Assert.Equal(now, result.CreatedAt);

        await _checker
            .Received(1)
            .Exists(
                organizationId,
                Arg.Any<CancellationToken>());

        _documentSession.Events
            .Received(1)
            .StartStream<HrAgencySystem.JobDescription.Domain.JobDescription>(
                result.JobDescriptionId,
                Arg.Is<JobDescriptionCreated>(x => x.JobDescriptionId == result.JobDescriptionId));
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationExceptionWithAllErrors()
    {
        var command = new CreateJobDescription(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            new string('A', LongText.MaxLength + 1),
            "",
            [
                "",
                new string('A', EntryText.MaxLength + 1)
            ],
            [
                "",
                new string('B', EntryText.MaxLength + 1)
            ],
            [
                "",
                new string('C', EntryText.MaxLength + 1)
            ],
            new string('A', JobLocation.MaxLength + 1),
            "POL",
            EmploymentType.FullTime,
            WorkMode.Remote,
            CurrencyCode.EUR,
            -1m,
            -2m,
            Guid.NewGuid(),
            RecruiterId);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.Equal(
            [
                JobTitle.RequiredMessage,
                LongText.MaxLengthMessage,
                LongText.FieldIsRequired("Job description"),
                JobLocation.MaxLengthMessage,
                EntryText.RequiredMessage,
                EntryText.MaxLengthMessage,
                EntryText.RequiredMessage,
                EntryText.MaxLengthMessage,
                EntryText.RequiredMessage,
                EntryText.MaxLengthMessage,
                SalaryRange.NegativeSalaryMessage,
                CountryCode.InvalidFormatMessage
            ],
            exception.Errors);

        await AssertNoOrganizationCheck();
        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithInvalidTitle_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(title: ""),
            JobTitle.RequiredMessage);
    }

    [Fact]
    public async Task Handle_WithTitleExceedingMaximumLength_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                title: new string('A', JobTitle.MaxLength + 1)),
            JobTitle.MaxLengthMessage);
    }

    [Fact]
    public async Task Handle_WithSummaryExceedingMaximumLength_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                summary: new string('A', LongText.MaxLength + 1)),
            LongText.MaxLengthMessage);
    }

    [Fact]
    public async Task Handle_WithDescriptionMissing_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(description: ""),
            LongText.FieldIsRequired("Job description"));
    }

    [Fact]
    public async Task Handle_WithDescriptionExceedingMaximumLength_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                description: new string('A', LongText.MaxLength + 1)),
            LongText.MaxLengthMessage);
    }

    [Fact]
    public async Task Handle_WithInvalidResponsibility_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                responsibilities:
                [
                    "Valid responsibility",
                    ""
                ]),
            EntryText.RequiredMessage);
    }

    [Fact]
    public async Task Handle_WithInvalidRequirement_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                requirements:
                [
                    "Valid requirement",
                    new string('A', EntryText.MaxLength + 1)
                ]),
            EntryText.MaxLengthMessage);
    }

    [Fact]
    public async Task Handle_WithInvalidSkill_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                skills:
                [
                    "C#",
                    ""
                ]),
            EntryText.RequiredMessage);
    }

    [Fact]
    public async Task Handle_WithInvalidLocation_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                location: new string('A', JobLocation.MaxLength + 1)),
            JobLocation.MaxLengthMessage);
    }

    [Fact]
    public async Task Handle_WithNegativeMinimumSalary_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                salaryMin: -1m,
                salaryMax: 10000m),
            SalaryRange.NegativeSalaryMessage);
    }

    [Fact]
    public async Task Handle_WithNegativeMaximumSalary_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                salaryMin: 1000m,
                salaryMax: -1m),
            SalaryRange.NegativeSalaryMessage);
    }

    [Fact]
    public async Task Handle_WithMinimumSalaryGreaterThanMaximum_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(
                salaryMin: 20000m,
                salaryMax: 10000m),
            SalaryRange.MinimumExceedsMaximumMessage);
    }

    [Fact]
    public async Task Handle_WithInvalidCountryCode_ThrowsValidationException()
    {
        await AssertValidationError(
            CreateValidCommand(countryCode: "POL"),
            CountryCode.InvalidFormatMessage);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrganization_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();

        _checker
            .Exists(organizationId, Arg.Any<CancellationToken>())
            .Returns(false);

        var exception = await AssertBusinessRuleError(
            CreateValidCommand(organizationId: organizationId));

        Assert.Equal(
            OrganizationId.OrganizationCheckMessage,
            exception.Message);

        await _checker
            .Received(1)
            .Exists(
                organizationId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ThrowsBusinessRuleException()
    {
        var recruiterId = Guid.NewGuid();

        _checker
            .Exists(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _snapshotRepository
            .GetUserAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        var exception = await AssertBusinessRuleError(
            CreateValidCommand(recruiterId: recruiterId));

        Assert.Equal(
            IUserSnapshotRepository.NotFoundMessage,
            exception.Message);

        await _snapshotRepository
            .Received(1)
            .GetUserAsync(
                recruiterId,
                Arg.Any<CancellationToken>());

        AssertNoStream();
    }

    private async Task AssertValidationError(
        CreateJobDescription command,
        string expectedError)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => Handle(command));

        Assert.Equal(
            [expectedError],
            exception.Errors);

        await AssertNoOrganizationCheck();
        AssertNoStream();
    }

    private async Task<BusinessRuleException> AssertBusinessRuleError(
        CreateJobDescription command)
    {
        return await Assert.ThrowsAsync<BusinessRuleException>(() => Handle(command));
    }

    private async Task<JobDescriptionCreated> Handle(
        CreateJobDescription command,
        IClock? clock = null)
    {
        return await CreateJobDescriptionHandler.Handle(
            command,
            _documentSession,
            clock ?? TestClock,
            _checker,
            _snapshotRepository,
            _companySnapshot,
            CancellationToken.None);
    }

    private async Task AssertNoOrganizationCheck()
    {
        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    private void AssertNoStream()
    {
        _documentSession.Events
            .DidNotReceive()
            .StartStream<HrAgencySystem.JobDescription.Domain.JobDescription>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    private static CreateJobDescription CreateValidCommand(
        Guid? organizationId = null,
        Guid? companyId = null,
        Guid? recruiterId = null,
        string title = "Senior .NET Developer",
        string? summary = "Senior developer position",
        string description =
            "We are looking for an experienced .NET developer.",
        IReadOnlyList<string>? responsibilities = null,
        IReadOnlyList<string>? requirements = null,
        IReadOnlyList<string>? skills = null,
        string location = "Opole",
        string countryCode = "PL",
        EmploymentType employmentType = EmploymentType.FullTime,
        WorkMode workMode = WorkMode.Hybrid,
        CurrencyCode currencyCode = CurrencyCode.PLN,
        decimal salaryMin = 15000m,
        decimal salaryMax = 22000m)
    {
        return new CreateJobDescription(
            organizationId ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            title,
            summary,
            description,
            responsibilities ??
            [
                "Design and develop backend services."
            ],
            requirements ??
            [
                "5+ years of .NET experience."
            ],
            skills ??
            [
                "C#",
                "ASP.NET Core"
            ],
            location,
            countryCode,
            employmentType,
            workMode,
            currencyCode,
            salaryMin,
            salaryMax,
            recruiterId ?? RecruiterId,
            RecruiterId);
    }

}

