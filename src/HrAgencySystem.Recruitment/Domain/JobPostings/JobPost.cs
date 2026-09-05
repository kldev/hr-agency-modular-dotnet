using HrAgencySystem.Recruitment.Domain.JobPostings.ValueObjects;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Domain.JobPostings;

public sealed class JobPost
{
    private JobPost(){}

    public static JobPost Empty()
    {
        return new JobPost();
    }
    
    public static JobPost WithOrganization(Guid organizationId)
    {
        var post = new JobPost
        {
            OrganizationId = new OrganizationId(organizationId)
        };
        return post;
    }
    
    public JobPostId Id { get; private set; }
    
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
    
    public JobPostStatus Status { get; private set; }

    public LanguageCode LanguageCode { get; set; } = null!;
    
    public Guid RecruiterId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
    
    public Guid CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; } = null;
    
    public IReadOnlyList<ChannelPost> Posts { get; private set; } = [];

    public void Apply(JobPostCreated @event)
    {
        Id = JobPostId.From(@event.JobPostId);
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

        Status = JobPostStatus.Draft;

        RecruiterId = @event.Recruiter.Id;

        CreatedBy = @event.CreatedBy.Id;
        CreatedAt = @event.CreatedAt;
        UpdatedAt = @event.CreatedAt;
        Posts = new List<ChannelPost>();

    }

    public void Apply(JobPostUpdated @event)
    {
        Title = PostTitle.Create(@event.Title);
        Summary = LongText.Create(@event.Summary ?? "");
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

        ApplyCommon(@event);
    }

    public void Apply(JobPostArchived @event)
    {
        Status = JobPostStatus.Archived;
        ApplyCommon(@event);
    }
    
    public void Apply(JobPostClosed @event)
    {
        Status = JobPostStatus.Closed;
        ApplyCommon(@event);
    }
    
    public void Apply(JobPostedToChannel @event)
    {
        RequireNotFinal();
        Status = JobPostStatus.Published;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Posts ??= new List<ChannelPost>();
        
        var posts = Posts.Append(new ChannelPost(@event.ChannelType, @event.OccurredAt));
        Posts = [.. posts];
        
        ApplyCommon(@event);
    }
    
    public void Apply(JobPostPublished @event)
    {
        Status = JobPostStatus.Published;
        ApplyCommon(@event);
    }

    public void Apply(JobPostRecruiterChanged @event)
    {
        RecruiterId = @event.Recruiter.Id;
        ApplyCommon(@event);
    }
    
    private void RequireNotFinal()
    {
        if (Status is
            JobPostStatus.Closed or
            JobPostStatus.Archived)
        {
            throw new InvalidOperationException(
                $"Application is already in final status: {Status}.");
        }
    }

    private void ApplyCommon(IJobPostEvent @event)
    {
        UpdatedAt = @event.OccurredAt;
        ModifiedBy = @event.Author.Id;
    }


}