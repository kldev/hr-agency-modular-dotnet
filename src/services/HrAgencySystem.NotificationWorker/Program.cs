using HrAgencySystem.EmailTemplates;
using HrAgencySystem.EmailTemplates.Messaging;
using HrAgencySystem.NotificationWorker.Infrastructure;
using Wolverine;

var builder = Host.CreateApplicationBuilder(args);
{
    builder.Services.AddEMailTemplates(builder.Configuration);
    builder.Services.AddNpgsqlDataSource(
        builder.Configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.")
    );
    builder.Services.AddSingleton<ProcessedEventMigration>();
    builder.Services.AddScoped<IProcessedEventStore, ProcessedEventStore>();

    var config = RabbitMqConfig.FromSection(
        builder.Configuration.GetSection(RabbitMqConfig.SectionName)
    );

    builder.UseWolverine(opts =>
    {
        opts.ConsumeEmailMessages(config);
        opts.RetryEmailDelivery();
        opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    });
}

var host = builder.Build();
{
    await host
        .Services.GetRequiredService<ProcessedEventMigration>()
        .MigrateAsync(CancellationToken.None);

    host.Run();
}
