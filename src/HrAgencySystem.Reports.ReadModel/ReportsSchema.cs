using Microsoft.EntityFrameworkCore;

namespace HrAgencySystem.Reports.ReadModel;

/// <summary>
/// The reporting read model: schema <c>reports</c>, one row per organization, job post,
/// application, interview and project, with the moments that matter as timestamps.
/// <para>
/// Each table is filled by an EF Core backed Marten projection in the module that owns the events
/// (the <c>Feeds</c> pattern) and read with plain SQL by the reports service. The contexts below are
/// the whole contract between them: they only describe and write the tables - Weasel migrates them
/// together with the projections, and nothing reads through them.
/// </para>
/// <para>
/// <b>One context per table, on purpose.</b> Every EF projection registers all the tables of its
/// context with Marten; five projections over one shared context put every index into the schema
/// migration five times, and the whole batch - the feed table included - fails on the duplicate.
/// </para>
/// </summary>
public static class ReportsSchema
{
    public const string Name = "reports";

    public static class Tables
    {
        public const string Organizations = "organizations";
        public const string JobPosts = "job_posts";
        public const string Applications = "applications";
        public const string Interviews = "interviews";
        public const string Projects = "projects";
    }
}

/// <summary><c>reports.organizations</c> - the tenant list, written by the Organization module.</summary>
public sealed class OrganizationsReportDbContext(
    DbContextOptions<OrganizationsReportDbContext> options
) : DbContext(options)
{
    // ReSharper disable once UnusedMember.Global
    public DbSet<OrganizationReportRow> Rows => Set<OrganizationReportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationReportRow>(entity =>
        {
            entity.ToTable(ReportsSchema.Tables.Organizations, ReportsSchema.Name);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(100);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });
    }
}

/// <summary><c>reports.job_posts</c> - job posts, written by the Recruitment module.</summary>
public sealed class JobPostsReportDbContext(DbContextOptions<JobPostsReportDbContext> options)
    : DbContext(options)
{
    // ReSharper disable once UnusedMember.Global
    public DbSet<JobPostReportRow> Rows => Set<JobPostReportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobPostReportRow>(entity =>
        {
            entity.ToTable(ReportsSchema.Tables.JobPosts, ReportsSchema.Name);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.IsPublished).HasColumnName("is_published");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.FirstPublishedAt).HasColumnName("first_published_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasIndex(x => new { x.OrganizationId, x.CreatedAt })
                .HasDatabaseName("idx_reports_job_posts_org_created");
        });
    }
}

/// <summary><c>reports.applications</c> - job applications, written by the Recruitment module.</summary>
public sealed class ApplicationsReportDbContext(
    DbContextOptions<ApplicationsReportDbContext> options
) : DbContext(options)
{
    // ReSharper disable once UnusedMember.Global
    public DbSet<ApplicationReportRow> Rows => Set<ApplicationReportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationReportRow>(entity =>
        {
            entity.ToTable(ReportsSchema.Tables.Applications, ReportsSchema.Name);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
            entity.Property(x => x.JobPostId).HasColumnName("job_post_id");
            entity.Property(x => x.Source).HasColumnName("source").HasMaxLength(40);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(40);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.ScreeningAt).HasColumnName("screening_at");
            entity.Property(x => x.InterviewAt).HasColumnName("interview_at");
            entity.Property(x => x.AssessmentAt).HasColumnName("assessment_at");
            entity.Property(x => x.OfferAt).HasColumnName("offer_at");
            entity.Property(x => x.HiredAt).HasColumnName("hired_at");
            entity.Property(x => x.RejectedAt).HasColumnName("rejected_at");
            entity.Property(x => x.WithdrawnAt).HasColumnName("withdrawn_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasIndex(x => new { x.OrganizationId, x.CreatedAt })
                .HasDatabaseName("idx_reports_applications_org_created");
        });
    }
}

/// <summary><c>reports.interviews</c> - interviews, written by the Recruitment module.</summary>
public sealed class InterviewsReportDbContext(DbContextOptions<InterviewsReportDbContext> options)
    : DbContext(options)
{
    // ReSharper disable once UnusedMember.Global
    public DbSet<InterviewReportRow> Rows => Set<InterviewReportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterviewReportRow>(entity =>
        {
            entity.ToTable(ReportsSchema.Tables.Interviews, ReportsSchema.Name);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
            entity.Property(x => x.JobApplicationId).HasColumnName("job_application_id");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(40);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasIndex(x => new { x.OrganizationId, x.CreatedAt })
                .HasDatabaseName("idx_reports_interviews_org_created");
        });
    }
}

/// <summary><c>reports.projects</c> - projects, written by the Projects module.</summary>
public sealed class ProjectsReportDbContext(DbContextOptions<ProjectsReportDbContext> options)
    : DbContext(options)
{
    // ReSharper disable once UnusedMember.Global
    public DbSet<ProjectReportRow> Rows => Set<ProjectReportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectReportRow>(entity =>
        {
            entity.ToTable(ReportsSchema.Tables.Projects, ReportsSchema.Name);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity
                .Property(x => x.EngagementType)
                .HasColumnName("engagement_type")
                .HasMaxLength(40);
            entity.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(40);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.WentLiveAt).HasColumnName("went_live_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasIndex(x => new { x.OrganizationId, x.CreatedAt })
                .HasDatabaseName("idx_reports_projects_org_created");
        });
    }
}
