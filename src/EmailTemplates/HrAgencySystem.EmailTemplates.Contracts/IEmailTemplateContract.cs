namespace HrAgencySystem.EmailTemplates.Contracts;

public interface IEmailTemplateContract
{
    Guid EventId { get; }
    string Source { get; }
}
