using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.EmailTemplates.Contracts.Recruitment;
using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.EmailTemplates.Contracts.Teams;
using Wolverine;
using Wolverine.RabbitMQ;

namespace HrAgencySystem.EmailTemplates.Messaging;

/// <summary>
/// The single description of the mail topology. Producers declare only the exchange they publish to,
/// consumers own their queues and bindings — both sides read the same constants, so the topology
/// cannot be declared two different ways.
/// </summary>
public static class EmailMessaging
{
    private const int DefaultListenerCount = 3;

    extension(WolverineOptions opts)
    {
        /// <summary>
        /// Sender side. Every mail message is routed by its own topic and goes through the durable
        /// outbox, so a broker outage cannot lose a mail that its transaction already committed.
        /// </summary>
        public void PublishEmailMessages(RabbitMqConfig config)
        {
            opts.UseRabbitMq(config.GetConnectionUri())
                .UseSenderConnectionOnly()
                .DeclareExchange(config.MailExchange, ex => ex.ExchangeType = ExchangeType.Topic)
                .AutoProvision();

            opts.PublishMessage<SendJobApplicationCreated>()
                .ToRabbitTopic(EmailTopics.JobApplicationCreated, config.MailExchange)
                .UseDurableOutbox();

            opts.PublishMessage<SendJobPostRecruiterChanged>()
                .ToRabbitTopic(EmailTopics.JobPostRecruiterChanged, config.MailExchange)
                .UseDurableOutbox();

            opts.PublishMessage<SendPasswordReset>()
                .ToRabbitTopic(EmailTopics.PasswordReset, config.MailExchange)
                .UseDurableOutbox();

            opts.PublishMessage<SendOpportunityCreated>()
                .ToRabbitTopic(EmailTopics.OpportunityCreated, config.MailExchange)
                .UseDurableOutbox();

            opts.PublishMessage<SendOpportunityResponsibleChanged>()
                .ToRabbitTopic(EmailTopics.OpportunityResponsibleChanged, config.MailExchange)
                .UseDurableOutbox();

            opts.PublishMessage<SendTeamMemberAdded>()
                .ToRabbitTopic(EmailTopics.TeamMemberAdded, config.MailExchange)
                .UseDurableOutbox();

            opts.PublishMessage<SendTeamMemberRoleChanged>()
                .ToRabbitTopic(EmailTopics.TeamMemberRoleChanged, config.MailExchange)
                .UseDurableOutbox();
        }

        /// <summary>
        /// Listener side. One durable queue per source domain, each bound with the domain wildcard so
        /// that a new topic within a domain reaches its consumer without touching the producer.
        /// </summary>
        public void ConsumeEmailMessages(
            RabbitMqConfig config,
            int listenerCount = DefaultListenerCount
        )
        {
            // No UseListenerConnectionOnly here: AutoProvision declares the topology over the
            // sending connection, and a listener-only node fails to start with it disabled.
            opts.UseRabbitMq(config.GetConnectionUri())
                .DeclareExchange(
                    config.MailExchange,
                    ex =>
                    {
                        ex.ExchangeType = ExchangeType.Topic;
                        ex.BindQueue(EmailQueues.Recruitment, EmailTopics.RecruitmentPattern);
                        ex.BindQueue(EmailQueues.Identity, EmailTopics.IdentityPattern);
                        ex.BindQueue(EmailQueues.Sales, EmailTopics.SalesPattern);
                        ex.BindQueue(EmailQueues.Teams, EmailTopics.TeamsPattern);
                    }
                )
                .DeclareQueue(EmailQueues.Recruitment, q => q.IsDurable = true)
                .DeclareQueue(EmailQueues.Identity, q => q.IsDurable = true)
                .DeclareQueue(EmailQueues.Sales, q => q.IsDurable = true)
                .DeclareQueue(EmailQueues.Teams, q => q.IsDurable = true)
                .AutoProvision();

            opts.ListenToRabbitQueue(EmailQueues.Recruitment).ListenerCount(listenerCount);
            opts.ListenToRabbitQueue(EmailQueues.Identity).ListenerCount(listenerCount);
            opts.ListenToRabbitQueue(EmailQueues.Sales).ListenerCount(listenerCount);
            opts.ListenToRabbitQueue(EmailQueues.Teams).ListenerCount(listenerCount);
        }
    }
}
