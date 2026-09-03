using HrAgencySystem.Recruitment.Domain.Posting.ValueObjects;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Domain.Posting;

public sealed class JobPosting
{
    private JobPosting(){}

    public JobPosting Empty()
    {
        return new JobPosting();
    }
    
    public JobPosting WithOrganization(Guid organizationId)
    {
        return new JobPosting();
    }
    
    public JobPostingId Id { get; private set; }
    
    public JobDescriptionId  DescriptionId { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public CompanyId CompanyId { get; private set; }
    
    public PostTitle Title { get; private set; } = null!;
    
    public LongText Summary { get; private set; } = null!;
    
    public LongText Description { get; private set; } = null!;
    
    public IReadOnlyList<EntryText> Responsibilities { get; private set; } = [];

    public IReadOnlyList<EntryText> Requirements { get; private set; } = [];

    public IReadOnlyList<EntryText> Skills { get; private set; } = [];
    
    public JobLocation Location { get; private set; } = null!;

    public CountryCode CountryCode { get; private set; } = null!;

    public EmploymentType EmploymentType { get; private set; }

    public WorkMode WorkMode { get; private set; }

    public SalaryRange SalaryRange { get; private set; } = null!;
    
    public JobPostingStatus Status { get; private set; }

    public LanguageCode LanguageCode { get; set; } = null!;
    
    public Guid RecruiterId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
    
    public Guid CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; } = null;

    public void Apply(JobPostCreated @event)
    {
        Id = JobPostingId.From(@event.JobPostingId);
        DescriptionId = JobDescriptionId.From(@event.JobDescriptionId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        CompanyId = CompanyId.From(@event.CompanyId);

        Title = PostTitle.Create(@event.Title);
        Summary = LongText.Create(@event.Summary);
        Description = LongText.Create(@event.Description);

        Responsibilities = EntryText.Create(@event.Responsibilities);
        Requirements = EntryText.Create(@event.Requirements);
        Skills = EntryText.Create(@event.Skills);

        Location = JobLocation.Create(@event.Location);
        CountryCode = CountryCode.Create(@event.CountryCode);
        LanguageCode = LanguageCode.Create(@event.LanguageCode);

        EmploymentType = @event.EmploymentType;
        WorkMode = @event.WorkMode;
        SalaryRange = SalaryRange.Create(@event.SalaryMin, @event.SalaryMax, @event.CurrencyCode);

        Status = JobPostingStatus.Draft;

        RecruiterId = @event.Recruiter.Id;

        CreatedBy = @event.CreatedBy.Id;
        CreatedAt = @event.CreatedAt;
        UpdatedAt = @event.CreatedAt;
    }
    
    
}