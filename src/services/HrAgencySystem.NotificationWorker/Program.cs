using HrAgencySystem.EmailTemplates;
using HrAgencySystem.EmailTemplates.Messaging;
using Wolverine;

var builder = Host.CreateApplicationBuilder(args);
{
    builder.Services.AddEMailTemplates(builder.Configuration);

    var config = RabbitMqConfig.FromSection(
        builder.Configuration.GetSection(RabbitMqConfig.SectionName)
    );

    builder.UseWolverine(opts =>
    {
        opts.ConsumeEmailMessages(config);
        opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    });
}

var host = builder.Build();
{
    host.Run();
}
