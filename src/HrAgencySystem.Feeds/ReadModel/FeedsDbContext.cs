using Microsoft.EntityFrameworkCore;

namespace HrAgencySystem.Feeds.ReadModel;

/// <summary>
/// Schema definition for the feed read model. EF Core is used only to describe and write the
/// table - Marten opens it on its own connection inside the projection transaction and Weasel
/// migrates it, so there is no <c>dotnet ef</c> step and nothing reads through this context.
/// </summary>
public sealed class FeedsDbContext(DbContextOptions<FeedsDbContext> options) : DbContext(options)
{
    public const string SchemaName = "feeds";
    public const string TableName = "job_posts";

    // ReSharper disable once UnusedMember.Global
    public DbSet<JobPostFeedRow> JobPosts => Set<JobPostFeedRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobPostFeedRow>(entity =>
        {
            entity.ToTable(TableName, SchemaName);
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
            entity.Property(x => x.IsPublished).HasColumnName("is_published");
            entity.Property(x => x.Title).HasColumnName("title");
            entity.Property(x => x.Summary).HasColumnName("summary");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Responsibilities).HasColumnName("responsibilities");
            entity.Property(x => x.Requirements).HasColumnName("requirements");
            entity.Property(x => x.Skills).HasColumnName("skills");
            entity.Property(x => x.Location).HasColumnName("location");
            entity.Property(x => x.LanguageCode).HasColumnName("language_code").HasMaxLength(2);
            entity.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2);
            entity.Property(x => x.PostingSlug).HasColumnName("posting_slug");
            entity.Property(x => x.SalaryMin).HasColumnName("salary_min");
            entity.Property(x => x.SalaryMax).HasColumnName("salary_max");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.Property(x => x.EmploymentType)
                .HasColumnName("employment_type")
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.Property(x => x.WorkMode)
                .HasColumnName("work_mode")
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.Property(x => x.CurrencyCode)
                .HasColumnName("currency_code")
                .HasConversion<string>()
                .HasMaxLength(3);

            entity.HasIndex(x => new { x.OrganizationId, x.IsPublished })
                .HasDatabaseName("idx_feeds_job_posts_org_published");
        });
    }
}
