namespace HrAgencySystem.EmailTemplates.Contracts;

public interface IEmailTemplateContract
{
    Guid EventId { get; }

    // ReSharper disable once UnusedMember.Global - part of every mail's wire contract: which domain sent it.
    string Source { get; }
}
