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

    private readonly IUserSnapshotService _snapshotService
        = Substitute.For<IUserSnapshotService>();

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsJobDescriptionCreated()
    {
        var organizationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var recruiter = GetRecruiter;
        

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
            recruiter.Id, recruiter.Id);

        _checker
            .Exists(
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _snapshotService.GetUserAsync(recruiter.Id, Arg.Any<CancellationToken>())
            .Returns(recruiter);

        var clock = new FixedClock(now);
        
        var result = await CreateJobDescriptionHandler.Handle(
            command,
            _documentSession,
            clock,
            _checker,
            _snapshotService,
            CancellationToken.None);

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
        Assert.Equal(CurrencyCode.PLN, result.SalaryRange.Currency);
        Assert.Equal(15000m, result.SalaryRange.Min);
        Assert.Equal(22000m, result.SalaryRange.Max);
        Assert.Equal(recruiter.Id, result.Recruiter.Id);
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
                Arg.Is<JobDescriptionCreated>(
                    x => x.JobDescriptionId == result.JobDescriptionId));
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationExceptionWithAllErrors()
    {
        var command = new CreateJobDescription(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            new string('A', JobSummary.MaxLength + 1),
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
            Guid.NewGuid(), GetRecruiter.Id);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [
                JobTitle.RequiredMessage,
                JobSummary.MaxLengthMessage,
                JobDescriptionText.RequiredMessage,
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

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<HrAgencySystem.JobDescription.Domain.JobDescription>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_WithInvalidTitle_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            title: "");

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [JobTitle.RequiredMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTitleExceedingMaximumLength_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            title: new string('A', JobTitle.MaxLength + 1));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [JobTitle.MaxLengthMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSummaryExceedingMaximumLength_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            summary: new string('A', JobSummary.MaxLength + 1));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [JobSummary.MaxLengthMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDescriptionMissing_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            description: "");

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [JobDescriptionText.RequiredMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDescriptionExceedingMaximumLength_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            description: new string('A', JobDescriptionText.MaxLength + 1));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [JobDescriptionText.MaxLengthMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidResponsibility_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            responsibilities:
            [
                "Valid responsibility",
                ""
            ]);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [EntryText.RequiredMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRequirement_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            requirements:
            [
                "Valid requirement",
                new string('A', EntryText.MaxLength + 1)
            ]);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [EntryText.MaxLengthMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidSkill_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            skills:
            [
                "C#",
                ""
            ]);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [EntryText.RequiredMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidLocation_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            location: new string('A', JobLocation.MaxLength + 1));

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [JobLocation.MaxLengthMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNegativeMinimumSalary_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            salaryMin: -1m,
            salaryMax: 10000m);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [SalaryRange.NegativeSalaryMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNegativeMaximumSalary_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            salaryMin: 1000m,
            salaryMax: -1m);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [SalaryRange.NegativeSalaryMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMinimumSalaryGreaterThanMaximum_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            salaryMin: 20000m,
            salaryMax: 10000m);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [SalaryRange.MinimumExceedsMaximumMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCountryCode_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            countryCode: "POL");

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            [CountryCode.InvalidFormatMessage],
            exception.Errors);

        await _checker
            .DidNotReceive()
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingOrganization_ThrowsBusinessRuleException()
    {
        var organizationId = Guid.NewGuid();

        var command = CreateValidCommand(
            organizationId: organizationId);

        _checker
            .Exists(
                organizationId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            OrganizationId.OrganizationCheckMessage,
            exception.Message);

        await _checker
            .Received(1)
            .Exists(
                organizationId,
                Arg.Any<CancellationToken>());

        _documentSession.Events
            .DidNotReceive()
            .StartStream<HrAgencySystem.JobDescription.Domain.JobDescription>(
                Arg.Any<Guid>(),
                Arg.Any<object>());
    }
    
    [Fact]
    public async Task Handle_WithNonExistingUser_ThrowsBusinessRuleException()
    {
        var recruiterId = Guid.NewGuid();

        var command = CreateValidCommand(
            recruiterId: recruiterId);

        _checker
            .Exists(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        _snapshotService.GetUserAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);
        
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateJobDescriptionHandler.Handle(
                command,
                _documentSession,
                TestClock,
                _checker,
                _snapshotService,
                CancellationToken.None));

        Assert.Equal(
            IUserSnapshotService.NotFoundMessage,
            exception.Message);

        await _snapshotService
            .Received(1)
            .GetUserAsync(
                recruiterId,
                Arg.Any<CancellationToken>());

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
        string description = "We are looking for an experienced .NET developer.",
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
            recruiterId ?? Guid.NewGuid(), Guid.NewGuid());
    }
    
    private static UserSnapshot GetRecruiter =>
        new (Guid.NewGuid(), "Alice", "Wells", "alice-wells@hr-agency.com");
}
