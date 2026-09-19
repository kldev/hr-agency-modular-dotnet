using HrAgencySystem.Api.Common.Config;
using HrAgencySystem.EmailTemplates.Contracts;
using Wolverine;
using Wolverine.RabbitMQ;

namespace HrAgencySystem.Api.Infrastructure;

public static class ConfigureWolverine
{
    extension(WolverineOptions opts)
    {
        public void AddRabbitMq(RabbitMqConfig config)
        {
            Console.WriteLine("Adding RabbitMQ at {0} with {1}", config.Host, config.MailExchange);

            opts.UseRabbitMq(config.GetConnectionUri())
                .UseSenderConnectionOnly()
                .DeclareExchange(
                    config.MailExchange,
                    ex =>
                    {
                        ex.BindQueue("queue1", config.RecruitmentRoutingKey);
                        ex.BindQueue("queue2", config.RecruitmentRoutingKey);
                        ex.BindQueue("queue3", config.RecruitmentRoutingKey);
                    }
                )
                .DeclareQueue(
                    "queue1",
                    o =>
                    {
                        o.IsDurable = true;
                    }
                )
                .DeclareQueue(
                    "queue2",
                    o =>
                    {
                        o.IsDurable = true;
                    }
                )
                .DeclareQueue(
                    "queue3",
                    o =>
                    {
                        o.IsDurable = true;
                    }
                )
                .AutoProvision();

            opts.UseRabbitMq(config.GetConnectionUri()).UseSenderConnectionOnly();

            opts.Publish(x =>
            {
                x.MessagesFromAssembly(typeof(IEmailTemplateContract).Assembly);
                x.ToRabbitRoutingKey(config.MailExchange, config.RecruitmentRoutingKey);
            });
        }
    }
}
