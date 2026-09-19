using Wolverine;
using Wolverine.RabbitMQ;

namespace HrAgencySystem.NotificationWorker;

public static class ConfigureWolverine
{
    extension(WolverineOptions opts)
    {
        public void AddRabbitMq(RabbitMqConfig config)
        {
            Console.WriteLine("Adding RabbitMQ at {0} with {1}", config.Host, config.MailExchange);

            opts.UseRabbitMq(config.GetConnectionUri())
                .UseListenerConnectionOnly()
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

            opts.ListenToRabbitQueue("queue1");
            opts.ListenToRabbitQueue("queue2");
            opts.ListenToRabbitQueue("queue3");

            opts.Discovery.IncludeAssembly(typeof(RabbitMqConfig).Assembly);
        }
    }
}
