using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.JobDescription.Domain;

public sealed class JobDescription
{
    private JobDescription()
    {
    }

    public JobDescriptionId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public CompanyId CompanyId { get; private set; }

    public JobTitle Title { get; private set; } = null!;

    public JobSummary Summary { get; private set; } = null!;

    public JobDescriptionText Description { get; private set; } = null!;

    public IReadOnlyList<EntryText> Responsibilities { get; private set; } = [];

    public IReadOnlyList<EntryText> Requirements { get; private set; } = [];

    public IReadOnlyList<EntryText> Skills { get; private set; } = [];

    public JobLocation Location { get; private set; } = null!;

    public CountryCode CountryCode { get; private set; } = null!;

    public EmploymentType EmploymentType { get; private set; }

    public WorkMode WorkMode { get; private set; }

    public SalaryRange SalaryRange { get; private set; }

    public JobDescriptionStatus Status { get; private set; }

    public Guid RecruiterId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
    
    public Guid CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; } = null;
    
    public static JobDescription Empty()
    {
        return new JobDescription();
    }

    public static JobDescription EmptyWithOrganizationId(OrganizationId organizationId)
    {
        return new JobDescription { OrganizationId = organizationId };
    }

    public void Apply(JobDescriptionCreated @event)
    {
        Id = JobDescriptionId.From(@event.JobDescriptionId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        CompanyId = CompanyId.From(@event.CompanyId);

        Title = JobTitle.Create(@event.Title);
        Summary = JobSummary.Create(@event.Summary);
        Description = JobDescriptionText.Create(@event.Description);

        Responsibilities = EntryText.Create(@event.Responsibilities);
        Requirements = EntryText.Create(@event.Requirements);
        Skills = EntryText.Create(@event.Skills);

        Location = JobLocation.Create(@event.Location);
        CountryCode = CountryCode.Create(@event.CountryCode);

        EmploymentType = @event.EmploymentType;
        WorkMode = @event.WorkMode;
        SalaryRange = @event.SalaryRange;

        Status = JobDescriptionStatus.Draft;

        RecruiterId = @event.Recruiter.Id;

        CreatedBy = @event.CreatedBy.Id;
        CreatedAt = @event.CreatedAt;
        UpdatedAt = @event.CreatedAt;
    }

    public void Apply(JobDescriptionUpdated @event)
    {
        Title = JobTitle.Create(@event.Title);
        Summary = JobSummary.Create(@event.Summary);
        Description = JobDescriptionText.Create(@event.Description);

        Responsibilities = EntryText.Create(@event.Responsibilities);
        Requirements = EntryText.Create(@event.Requirements);
        Skills = EntryText.Create(@event.Skills);

        Location = JobLocation.Create(@event.Location);
        CountryCode = CountryCode.Create(@event.CountryCode);

        EmploymentType = @event.EmploymentType;
        WorkMode = @event.WorkMode;
        SalaryRange = @event.SalaryRange;

        UpdatedAt = @event.UpdatedAt;
        ModifiedBy = @event.ModifiedBy.Id;
    }

    public void Apply(JobDescriptionOpened @event)
    {
        Status = JobDescriptionStatus.Open;
        UpdatedAt = @event.OccurredAt;
        ModifiedBy = @event.ModifiedBy.Id;
    }

    public void Apply(JobDescriptionPutOnHold @event)
    {
        Status = JobDescriptionStatus.OnHold;
        UpdatedAt = @event.OccurredAt;
        ModifiedBy = @event.ModifiedBy.Id;
    }

    public void Apply(JobDescriptionClosed @event)
    {
        Status = JobDescriptionStatus.Closed;
        UpdatedAt = @event.OccurredAt;
        ModifiedBy = @event.ModifiedBy.Id;
    }

    public void Apply(JobDescriptionCancelled @event)
    {
        Status = JobDescriptionStatus.Cancelled;
        UpdatedAt = @event.OccurredAt;
        ModifiedBy = @event.ModifiedBy.Id;
    }
}