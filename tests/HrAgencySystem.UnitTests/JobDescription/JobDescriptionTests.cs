using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using D = HrAgencySystem.JobDescription.Domain;

namespace HrAgencySystem.UnitTests.JobDescription;

public class JobDescriptionTests
{
    private static UserSnapshot User { get; } = new (Guid.NewGuid(), "Test", "User", "test@test.io");
    private UserSnapshot ModifiedBy { get; } = new (Guid.NewGuid(), "Other", "Other", "other@test.io");
    private static UserSnapshot Recruiter { get; } =
        new (Guid.NewGuid(), "Alice", "Wells", "alice-wells@hr-agency.com");
    
    private static CompanySnapshot Company { get; } = new (Guid.NewGuid(), "Company A", "TX100");
    
    [Fact]
    public void Empty_ReturnsEmptyJobDescription()
    {
        var jobDescription = D.JobDescription.Empty();

        Assert.NotNull(jobDescription);
        Assert.Equal(default, jobDescription.Id);
        Assert.Equal(default, jobDescription.OrganizationId);
        Assert.Equal(default, jobDescription.CompanyId);
        Assert.Null(jobDescription.Title);
        Assert.Null(jobDescription.CountryCode);
        Assert.Empty(jobDescription.Responsibilities);
        Assert.Empty(jobDescription.Requirements);
        Assert.Empty(jobDescription.Skills);
    }

    [Fact]
    public void Apply_JobDescriptionCreated_InitializesAggregate()
    {
        var jobDescriptionId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var recruiter = Recruiter; 
        var occurredAt = new DateTimeOffset(
            2026, 9, 1, 10, 0, 0, TimeSpan.Zero);

        var salaryRange = SalaryRange.Create(
            15000m,
            22000m,
            CurrencyCode.PLN);

        var @event = new JobDescriptionCreated(
            jobDescriptionId,
            organizationId,
            companyId,
            "Senior .NET Developer",
            "Senior developer position",
            "We are looking for an experienced .NET developer.",
            [
                "Design and develop backend services.",
                "Review code."
            ],
            [
                "5+ years of .NET experience.",
                "Experience with PostgreSQL."
            ],
            [
                "C#",
                "ASP.NET Core"
            ],
            "Opole",
            "PL",
            EmploymentType.FullTime,
            WorkMode.Hybrid,
            salaryRange.Currency,
            salaryRange.Min,
            salaryRange.Max,
            recruiter,
            User,
            Company,
            occurredAt);

        var jobDescription = D.JobDescription.Empty();

        jobDescription.Apply(@event);

        Assert.Equal(
            JobDescriptionId.From(jobDescriptionId),
            jobDescription.Id);

        Assert.Equal(
            OrganizationId.From(organizationId),
            jobDescription.OrganizationId);

        Assert.Equal(
            CompanyId.From(companyId),
            jobDescription.CompanyId);

        Assert.Equal(
            JobTitle.Create("Senior .NET Developer"),
            jobDescription.Title);

        Assert.Equal(
            JobSummary.Create("Senior developer position"),
            jobDescription.Summary);

        Assert.Equal(
            JobDescriptionText.Create(
                "We are looking for an experienced .NET developer."),
            jobDescription.Description);

        Assert.Equal(
            EntryText.Create(
            [
                "Design and develop backend services.",
                "Review code."
            ]),
            jobDescription.Responsibilities);

        Assert.Equal(
            EntryText.Create(
            [
                "5+ years of .NET experience.",
                "Experience with PostgreSQL."
            ]),
            jobDescription.Requirements);

        Assert.Equal(
            EntryText.Create(
            [
                "C#",
                "ASP.NET Core"
            ]),
            jobDescription.Skills);

        Assert.Equal(
            JobLocation.Create("Opole"),
            jobDescription.Location);

        Assert.Equal(
            CountryCode.Create("PL"),
            jobDescription.CountryCode);

        Assert.Equal(
            EmploymentType.FullTime,
            jobDescription.EmploymentType);

        Assert.Equal(
            WorkMode.Hybrid,
            jobDescription.WorkMode);

        Assert.Equal(
            salaryRange,
            jobDescription.SalaryRange);

        Assert.Equal(
            JobDescriptionStatus.Draft,
            jobDescription.Status);

        Assert.Equal(
            recruiter.Id,
            jobDescription.RecruiterId);

        Assert.Equal(
            occurredAt,
            jobDescription.CreatedAt);

        Assert.Equal(
            occurredAt,
            jobDescription.UpdatedAt);
    }

    [Fact]
    public void Apply_JobDescriptionUpdated_UpdatesJobDescriptionData()
    {
        var jobDescription = CreateJobDescription();

        var updatedAt = new DateTimeOffset(
            2026, 9, 2, 12, 30, 0, TimeSpan.Zero);

        var salaryRange = SalaryRange.Create(
            18000m,
            25000m,
            CurrencyCode.EUR);

        var @event = new JobDescriptionUpdated(
            "Lead .NET Developer",
            "Lead developer position",
            "Updated job description.",
            [
                "Lead backend development."
            ],
            [
                "7+ years of .NET experience."
            ],
            [
                "C#",
                "ASP.NET Core",
                "PostgreSQL"
            ],
            "Berlin",
            "DE",
            EmploymentType.Contract,
            WorkMode.Remote,
            salaryRange.Currency,
            salaryRange.Min,
            salaryRange.Max,
            User,
            updatedAt);

        var originalId = jobDescription.Id;
        var originalOrganizationId = jobDescription.OrganizationId;
        var originalCompanyId = jobDescription.CompanyId;
        var originalRecruiterId = jobDescription.RecruiterId;
        var originalCreatedAt = jobDescription.CreatedAt;

        jobDescription.Apply(@event);

        Assert.Equal(originalId, jobDescription.Id);
        Assert.Equal(
            originalOrganizationId,
            jobDescription.OrganizationId);

        Assert.Equal(
            originalCompanyId,
            jobDescription.CompanyId);

        Assert.Equal(
            originalRecruiterId,
            jobDescription.RecruiterId);

        Assert.Equal(
            originalCreatedAt,
            jobDescription.CreatedAt);

        Assert.Equal(
            JobTitle.Create("Lead .NET Developer"),
            jobDescription.Title);

        Assert.Equal(
            JobSummary.Create("Lead developer position"),
            jobDescription.Summary);

        Assert.Equal(
            JobDescriptionText.Create("Updated job description."),
            jobDescription.Description);

        Assert.Equal(
            EntryText.Create(
            [
                "Lead backend development."
            ]),
            jobDescription.Responsibilities);

        Assert.Equal(
            EntryText.Create(
            [
                "7+ years of .NET experience."
            ]),
            jobDescription.Requirements);

        Assert.Equal(
            EntryText.Create(
            [
                "C#",
                "ASP.NET Core",
                "PostgreSQL"
            ]),
            jobDescription.Skills);

        Assert.Equal(
            JobLocation.Create("Berlin"),
            jobDescription.Location);

        Assert.Equal(
            CountryCode.Create("DE"),
            jobDescription.CountryCode);

        Assert.Equal(
            EmploymentType.Contract,
            jobDescription.EmploymentType);

        Assert.Equal(
            WorkMode.Remote,
            jobDescription.WorkMode);

        Assert.Equal(
            salaryRange,
            jobDescription.SalaryRange);

        Assert.Equal(
            updatedAt,
            jobDescription.UpdatedAt);

        Assert.Equal(
            JobDescriptionStatus.Draft,
            jobDescription.Status);
    }

    [Fact]
    public void Apply_JobDescriptionOpened_SetsStatusToOpen()
    {
        var jobDescription = CreateJobDescription();

        var occurredAt = new DateTimeOffset(
            2026, 9, 2, 10, 0, 0, TimeSpan.Zero);

        var @event = new JobDescriptionOpened(jobDescription.Id.Value, ModifiedBy,
            occurredAt);

        jobDescription.Apply(@event);

        Assert.Equal(
            JobDescriptionStatus.Open,
            jobDescription.Status);

        Assert.Equal(
            occurredAt,
            jobDescription.UpdatedAt);

        Assert.Equal(ModifiedBy.Id, jobDescription.ModifiedBy);
    }

    [Fact]
    public void Apply_JobDescriptionPutOnHold_SetsStatusToOnHold()
    {
        var jobDescription = CreateJobDescription();

        var occurredAt = new DateTimeOffset(
            2026, 9, 2, 11, 0, 0, TimeSpan.Zero);

        var @event = new JobDescriptionPutOnHold(jobDescription.Id.Value, ModifiedBy,
            occurredAt);

        jobDescription.Apply(@event);

        Assert.Equal(
            JobDescriptionStatus.OnHold,
            jobDescription.Status);

        Assert.Equal(
            occurredAt,
            jobDescription.UpdatedAt);
        
        Assert.Equal(ModifiedBy.Id, jobDescription.ModifiedBy);
    }

    [Fact]
    public void Apply_JobDescriptionClosed_SetsStatusToClosed()
    {
        var jobDescription = CreateJobDescription();

        var occurredAt = new DateTimeOffset(
            2026, 9, 2, 12, 0, 0, TimeSpan.Zero);

        var @event = new JobDescriptionClosed(jobDescription.Id.Value, ModifiedBy,
            occurredAt);

        jobDescription.Apply(@event);

        Assert.Equal(
            JobDescriptionStatus.Closed,
            jobDescription.Status);

        Assert.Equal(
            occurredAt,
            jobDescription.UpdatedAt);
        
        Assert.Equal(ModifiedBy.Id, jobDescription.ModifiedBy);
    }

    [Fact]
    public void Apply_JobDescriptionCancelled_SetsStatusToCancelled()
    {
        var jobDescription = CreateJobDescription();

        var occurredAt = new DateTimeOffset(
            2026, 9, 2, 13, 0, 0, TimeSpan.Zero);

        var @event = new JobDescriptionCancelled(jobDescription.Id.Value, ModifiedBy,
            occurredAt);

        jobDescription.Apply(@event);

        Assert.Equal(
            JobDescriptionStatus.Cancelled,
            jobDescription.Status);

        Assert.Equal(
            occurredAt,
            jobDescription.UpdatedAt);
        
        Assert.Equal(ModifiedBy.Id, jobDescription.ModifiedBy);
    }

    [Fact]
    public void Apply_JobDescriptionOpened_DoesNotChangeCreatedAt()
    {
        var jobDescription = CreateJobDescription();

        var createdAt = jobDescription.CreatedAt;

        var occurredAt = createdAt.AddHours(2);

        var @event = new JobDescriptionOpened(jobDescription.Id.Value, ModifiedBy,
            occurredAt);

        jobDescription.Apply(@event);

        Assert.Equal(
            createdAt,
            jobDescription.CreatedAt);

        Assert.Equal(
            occurredAt,
            jobDescription.UpdatedAt);
        
        Assert.Equal(ModifiedBy.Id, jobDescription.ModifiedBy);
    }

    [Fact]
    public void Apply_JobDescriptionUpdated_DoesNotChangeStatus()
    {
        var jobDescription = CreateJobDescription();

        var openedAt = jobDescription.CreatedAt.AddHours(1);

        jobDescription.Apply(
            new JobDescriptionOpened(jobDescription.Id.Value, ModifiedBy,
                openedAt));

        var updatedAt = openedAt.AddHours(1);
        var salaryRange = SalaryRange.Create(
            10000m,
            15000m,
            CurrencyCode.PLN);

        var @event = new JobDescriptionUpdated(
            "Updated title",
            "Updated summary",
            "Updated description",
            ["Responsibility"],
            ["Requirement"],
            ["C#"],
            "Opole",
            "PL",
            EmploymentType.FullTime,
            WorkMode.Hybrid,
            salaryRange.Currency,
            salaryRange.Min,
            salaryRange.Max,
            ModifiedBy,
            updatedAt);

        jobDescription.Apply(@event);

        Assert.Equal(
            JobDescriptionStatus.Open,
            jobDescription.Status);

        Assert.Equal(
            updatedAt,
            jobDescription.UpdatedAt);
        
        Assert.Equal(ModifiedBy.Id, jobDescription.ModifiedBy);
    }


    
    private static D.JobDescription CreateJobDescription()
    {
        var recruiter = Recruiter;
        var salaryRange = SalaryRange.Create(
            15000m,
            22000m,
            CurrencyCode.PLN);
        
        var @event = new JobDescriptionCreated(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Senior .NET Developer",
            "Senior developer position",
            "We are looking for an experienced .NET developer.",
            [
                "Design and develop backend services.",
                "Review code."
            ],
            [
                "5+ years of .NET experience."
            ],
            [
                "C#",
                "ASP.NET Core"
            ],
            "Opole",
            "PL",
            EmploymentType.FullTime,
            WorkMode.Hybrid,
            
            salaryRange.Currency,
            salaryRange.Min,
            salaryRange.Max,
            recruiter,
            User,
            Company,
            new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero));

        var jobDescription = D.JobDescription.Empty();

        jobDescription.Apply(@event);

        return jobDescription;
    }
}
