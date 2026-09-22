using HrAgencySystem.Feeds;
using HrAgencySystem.Files;
using HrAgencySystem.Files.Service;
using HrAgencySystem.Observability;
using HrAgencySystem.Observability.Health;
using HrAgencySystem.Organization;
using HrAgencySystem.SharedKernel.Time;
using JasperFx;
using JasperFx.Events;
using Marten;

var builder = Host.CreateApplicationBuilder(args);
{
    builder.AddObservability("hr-feeds-worker");

    var connectionString =
        builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
    builder.Services.AddNpgsqlDataSource(connectionString);

    builder.Services.AddTransient<IClock, SystemClock>();

    builder.Services.AddFeedsModule(builder.Configuration);
    builder.Services.AddFeedsBackgroundWorkers();
    builder.Services.AddFilesModule(builder.Configuration);
    builder.Services.AddOrganizationModule(builder.Configuration);
    builder
        .Services.AddHealthChecks()
        .AddNpgSql(name: "postgres", tags: HealthTags.ReadyOnly, timeout: TimeSpan.FromSeconds(5))
        .AddObjectStorage(FeedBuckets.Jobs, HealthTags.ReadyOnly);

    /*
     * Marten is here for one reason: the scheduler asks the Organization module which organizations
     * are active, and that module answers from its own projection. Feed content itself is read
     * straight from feeds.job_posts with Dapper.
     *
     * No async daemon and no schema creation - the API owns both. A worker started against an empty
     * database waits for the API to create the schema instead of racing it.
     */
    builder.Services.AddMarten(options =>
    {
        options.Connection(connectionString);

        options.Events.DatabaseSchemaName = "events";
        options.Events.StreamIdentity = StreamIdentity.AsGuid;

        OrganizationModule.ConfigureMarten(options);

        options.AutoCreateSchemaObjects = AutoCreate.None;
    });
}

var host = builder.Build();
{
    host.Run();
}
