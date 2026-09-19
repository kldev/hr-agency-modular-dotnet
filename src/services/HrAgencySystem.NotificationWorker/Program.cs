using HrAgencySystem.EmailTemplates;
using HrAgencySystem.NotificationWorker;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
{
    builder.Services.AddOptions<RabbitMqConfig>(RabbitMqConfig.SectionName);
    builder.Services.AddHostedService<Worker>();
    builder.Services.AddEMailTemplates();

    var section = builder.Configuration.GetSection(RabbitMqConfig.SectionName);

    var config = RabbitMqConfig.FromSection(section);

    Console.WriteLine($"Using RabbitMQ connection string: {config.GetConnectionUri()}");

    builder.UseWolverine(opts =>
    {
        opts.AddRabbitMq(config);
    });
}

var host = builder.Build();
{
    host.Run();
}
