namespace HrAgencySystem.EmailTemplates.Contracts.Sales;

/// <summary>
/// Sent only when an opportunity is created for somebody else; opening one for yourself is not news
/// to anybody.
/// </summary>
public sealed record SendOpportunityCreated(
    Guid EventId,
    string Source,
    Guid OpportunityId,
    string CompanyName,
    Guid CompanyId,
    string ResponsiblePersonFullName,
    string ResponsiblePersonEmail,
    string OpportunityTitle
) : IEmailTemplateContract;
