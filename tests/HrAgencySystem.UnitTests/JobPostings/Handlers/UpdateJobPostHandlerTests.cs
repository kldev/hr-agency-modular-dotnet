using HrAgencySystem.Recruitment.Application.JobPosting.Update;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Domain.JobPostings.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using NSubstitute;

namespace HrAgencySystem.UnitTests.JobPostings.Handlers;

public class UpdateJobPostHandlerTests : BaseTest
{
    private readonly IUserSnapshotRepository _snapshotRepository =
        Substitute.For<IUserSnapshotRepository>();

    private static UserSnapshot ModifiedBy { get; } =
        new(
            Guid.NewGuid(),
            "Test",
            "User",
            "test@test.io");

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsJobPostUpdated()
    {
        var jobPostId = Guid.NewGuid();

        var now = new DateTimeOffset(
            2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        var command = CreateValidCommand(
            jobPostId: jobPostId,
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

        var aggregate = JobPost.WithOrganization(command.OrganizationId);

        var clock = new FixedClock(now);

        _snapshotRepository
            .GetUserAsync(
                command.ModifiedBy,
                Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);

        var result = (await UpdateJobPostHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            clock,
            CancellationToken.None)).Item1;

        Assert.Equal(
            jobPostId,
            result.JobPostId);

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

        Assert.Same(
            ModifiedBy,
            result.Author);

        Assert.Equal(
            now,
            result.OccurredAt);

        await _snapshotRepository
            .Received(1)
            .GetUserAsync(
                command.ModifiedBy,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidData_ThrowsValidationExceptionWithAllErrors()
    {
        var command = new UpdateJobPost(
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
            "001",
            EmploymentType.FullTime,
            WorkMode.Remote,
            CurrencyCode.EUR,
            -1m,
            -2m,
            Guid.NewGuid());

        var aggregate = JobPost.WithOrganization(command.OrganizationId);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            UpdateJobPostHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock,
                CancellationToken.None));

        Assert.Equal(
            [
                PostTitle.RequiredMessage,
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
                CountryCode.InvalidFormatMessage,
                LanguageCode.InvalidFormatMessage
            ],
            exception.Errors);

        await _snapshotRepository
            .DidNotReceive()
            .GetUserAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingModifiedByUser_ThrowsBusinessRuleException()
    {
        var modifiedBy = Guid.NewGuid();

        var command = CreateValidCommand(
            modifiedBy: modifiedBy);

        var aggregate = JobPost.WithOrganization(command.OrganizationId);

        _snapshotRepository
            .GetUserAsync(
                modifiedBy,
                Arg.Any<CancellationToken>())
            .Returns((UserSnapshot?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            UpdateJobPostHandler.Handle(
                command,
                aggregate,
                _snapshotRepository,
                TestClock,
                CancellationToken.None));

        Assert.Equal(
            IUserSnapshotRepository.NotFoundMessage,
            exception.Message);

        await _snapshotRepository
            .Received(1)
            .GetUserAsync(
                modifiedBy,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDifferentEmploymentTypeAndWorkMode_ReturnsUpdatedValues()
    {
        var command = CreateValidCommand(
            employmentType: EmploymentType.PartTime,
            workMode: WorkMode.Remote);

        var aggregate = JobPost.WithOrganization(command.OrganizationId);

        _snapshotRepository
            .GetUserAsync(
                command.ModifiedBy,
                Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);

        var result = (await UpdateJobPostHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            TestClock,
            CancellationToken.None)).Item1;

        Assert.Equal(
            EmploymentType.PartTime,
            result.EmploymentType);

        Assert.Equal(
            WorkMode.Remote,
            result.WorkMode);
    }

    [Fact]
    public async Task Handle_WithDifferentCurrency_ReturnsUpdatedSalaryRange()
    {
        var command = CreateValidCommand(
            currencyCode: CurrencyCode.EUR,
            salaryMin: 5000m,
            salaryMax: 8000m);

        var aggregate = JobPost.WithOrganization(command.OrganizationId);

        _snapshotRepository
            .GetUserAsync(
                command.ModifiedBy,
                Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);

        var result = (await UpdateJobPostHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            TestClock,
            CancellationToken.None)).Item1;

        Assert.Equal(
            CurrencyCode.EUR,
            result.CurrencyCode);

        Assert.Equal(
            5000m,
            result.SalaryMin);

        Assert.Equal(
            8000m,
            result.SalaryMax);
    }

    [Fact]
    public async Task Handle_WithClock_ReturnsEventWithCurrentTime()
    {
        var now = new DateTimeOffset(
            2026, 9, 2, 8, 30, 45, TimeSpan.FromHours(2));

        var command = CreateValidCommand();

        var aggregate = JobPost.WithOrganization(command.OrganizationId);

        var clock = new FixedClock(now);

        _snapshotRepository
            .GetUserAsync(
                command.ModifiedBy,
                Arg.Any<CancellationToken>())
            .Returns(ModifiedBy);

        var result = (await UpdateJobPostHandler.Handle(
            command,
            aggregate,
            _snapshotRepository,
            clock,
            CancellationToken.None)).Item1;

        Assert.Equal(
            now,
            result.OccurredAt);
    }

    private static UpdateJobPost CreateValidCommand(
        Guid? jobPostId = null,
        Guid? organizationId = null,
        string title = "Senior .NET Developer",
        string? summary = "Senior developer position",
        string description =
            "We are looking for an experienced .NET developer.",
        IReadOnlyList<string>? responsibilities = null,
        IReadOnlyList<string>? requirements = null,
        IReadOnlyList<string>? skills = null,
        string location = "Opole",
        string countryCode = "PL",
        string languageCode = "PL",
        EmploymentType employmentType = EmploymentType.FullTime,
        WorkMode workMode = WorkMode.Hybrid,
        CurrencyCode currencyCode = CurrencyCode.PLN,
        decimal salaryMin = 15000m,
        decimal salaryMax = 22000m,
        Guid? modifiedBy = null)
    {
        return new UpdateJobPost(
            jobPostId ?? Guid.NewGuid(),
            organizationId ?? Guid.NewGuid(),
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
            languageCode,
            employmentType,
            workMode,
            currencyCode,
            salaryMin,
            salaryMax,
            modifiedBy ?? ModifiedBy.Id);
    }
}