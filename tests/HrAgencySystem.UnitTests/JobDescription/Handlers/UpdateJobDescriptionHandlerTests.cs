using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Application.Handlers;
using D = HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using NSubstitute;

namespace HrAgencySystem.UnitTests.JobDescription.Handlers;

public class UpdateJobDescriptionHandlerTests : BaseTest
{
    private readonly IUserSnapshotRepository _snapshotRepository =
        Substitute.For<IUserSnapshotRepository>();
    private UserSnapshot ModifiedBy { get; } = new (Guid.NewGuid(), "Test", "User", "test@test.io");
    
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsJobDescriptionUpdated()
    {
        var jobDescriptionId = Guid.NewGuid();

        var now = new DateTimeOffset(
            2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        var command = CreateValidCommand(
            jobDescriptionId: jobDescriptionId,
            title: "  Senior .NET Developer  ",
            summary: "  Senior developer position  ",
            description: "  We are looking for an experienced .NET developer.  ",
            responsibilities:
            [
                "  Design and develop backend services.  ",
                "  Review code.  "
            ],
            requirements:
            [
                "  5+ years of .NET experience.  ",
                "  Experience with PostgreSQL.  "
            ],
            skills:
            [
                "  C#  ",
                "  ASP.NET Core  "
            ],
            location: "  Opole  ",
            countryCode: "pl",
            employmentType: EmploymentType.FullTime,
            workMode: WorkMode.Hybrid,
            currencyCode: CurrencyCode.PLN,
            salaryMin: 15000m,
            salaryMax: 22000m);

        var aggregate = D.JobDescription.EmptyWithOrganizationId(new OrganizationId(command.OrganizationId));

        var clock = new FixedClock(now);

        _snapshotRepository.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(ModifiedBy);
        
        var (result, events) = await UpdateJobDescriptionHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            clock, CancellationToken.None);

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

        Assert.Equal("Opole", result.Location);
        Assert.Equal("PL", result.CountryCode);
        Assert.Equal(EmploymentType.FullTime, result.EmploymentType);
        Assert.Equal(WorkMode.Hybrid, result.WorkMode);

        Assert.Equal(
            CurrencyCode.PLN,
            result.CurrencyCode);

        Assert.Equal(
            15000m,
            result.SalaryMin);

        Assert.Equal(
            22000m,
            result.SalaryMax);

        Assert.Equal(now, result.UpdatedAt);

        Assert.Single(events);
        Assert.Same(result, events[0]);
    }

    [Fact]
    public async Task Handle_WithNullAggregate_ThrowsNotFoundException()
    {
        var jobDescriptionId = Guid.NewGuid();

        var command = CreateValidCommand(
            jobDescriptionId: jobDescriptionId);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                null!,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            "Not found " + jobDescriptionId,
            exception.Message);
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationExceptionWithAllErrors()
    {
        var command = new UpdateJobDescription(
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
            -2m, Guid.NewGuid());

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock,
                CancellationToken.None
                ));

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
    }

    [Fact]
    public async Task Handle_WithInvalidTitle_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            title: "");

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [JobTitle.RequiredMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithTitleExceedingMaximumLength_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            title: new string('A', JobTitle.MaxLength + 1));

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [JobTitle.MaxLengthMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithSummaryExceedingMaximumLength_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            summary: new string('A', LongText.MaxLength + 1));

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [LongText.MaxLengthMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithDescriptionMissing_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            description: "");

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [LongText.FieldIsRequired("Job description")],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithDescriptionExceedingMaximumLength_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            description: new string('A', LongText.MaxLength + 1));

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [LongText.MaxLengthMessage],
            exception.Errors);
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

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [EntryText.RequiredMessage],
            exception.Errors);
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

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [EntryText.MaxLengthMessage],
            exception.Errors);
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

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [EntryText.RequiredMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithInvalidLocation_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            location: new string('A', JobLocation.MaxLength + 1));

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [JobLocation.MaxLengthMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithNegativeMinimumSalary_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            salaryMin: -1m,
            salaryMax: 10000m);

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [SalaryRange.NegativeSalaryMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithNegativeMaximumSalary_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            salaryMin: 1000m,
            salaryMax: -1m);

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [SalaryRange.NegativeSalaryMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithMinimumSalaryGreaterThanMaximum_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            salaryMin: 20000m,
            salaryMax: 10000m);

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [SalaryRange.MinimumExceedsMaximumMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithInvalidCountryCode_ThrowsValidationException()
    {
        var command = CreateValidCommand(
            countryCode: "POL");

        var aggregate = D.JobDescription.Empty();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobDescriptionHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock, CancellationToken.None));

        Assert.Equal(
            [CountryCode.InvalidFormatMessage],
            exception.Errors);
    }

    [Fact]
    public async Task Handle_WithDifferentEmploymentTypeAndWorkMode_ReturnsUpdatedValues()
    {
        var command = CreateValidCommand(
            employmentType: EmploymentType.PartTime,
            workMode: WorkMode.Remote);

        var aggregate = D.JobDescription.EmptyWithOrganizationId(new OrganizationId(command.OrganizationId));

        _snapshotRepository.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(ModifiedBy);
        
        var (result, events) = await UpdateJobDescriptionHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            TestClock, CancellationToken.None);

        Assert.Equal(
            EmploymentType.PartTime,
            result.EmploymentType);

        Assert.Equal(
            WorkMode.Remote,
            result.WorkMode);

        Assert.Single(events);
        Assert.Same(result, events[0]);
    }

    [Fact]
    public async Task Handle_WithDifferentCurrency_ReturnsUpdatedSalaryRange()
    {
        var command = CreateValidCommand(
            currencyCode: CurrencyCode.EUR,
            salaryMin: 5000m,
            salaryMax: 8000m);

        var aggregate = D.JobDescription.EmptyWithOrganizationId(new OrganizationId(command.OrganizationId));
        
        _snapshotRepository.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(ModifiedBy);

        var (result, _) = await UpdateJobDescriptionHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            TestClock, CancellationToken.None);

        Assert.Equal(
            CurrencyCode.EUR,
            result.CurrencyCode);

        Assert.Equal(5000m, result.SalaryMin);
        Assert.Equal(8000m, result.SalaryMax);
    }

    [Fact]
    public async Task Handle_WithClock_ReturnsEventWithCurrentTime()
    {
        var now = new DateTimeOffset(
            2026, 9, 2, 8, 30, 45, TimeSpan.FromHours(2));

        var command = CreateValidCommand();

        var aggregate = D.JobDescription.EmptyWithOrganizationId(new OrganizationId(command.OrganizationId));

        var clock = new FixedClock(now);

        _snapshotRepository.GetUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(ModifiedBy);

        var (result, _) = await UpdateJobDescriptionHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            clock, CancellationToken.None);

        Assert.Equal(now, result.UpdatedAt);
    }

    private static UpdateJobDescription CreateValidCommand(
        Guid? jobDescriptionId = null,
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
        return new UpdateJobDescription(
            jobDescriptionId ?? Guid.NewGuid(),
            Guid.NewGuid(),
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
            salaryMax, Guid.NewGuid());
    }
}