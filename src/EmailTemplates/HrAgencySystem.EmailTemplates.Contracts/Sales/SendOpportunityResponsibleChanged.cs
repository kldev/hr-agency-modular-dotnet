namespace HrAgencySystem.EmailTemplates.Contracts.Sales;

/// <summary>
/// Sent only when somebody hands an opportunity over to another person; taking it over yourself is
/// not news to anybody.
/// </summary>
public sealed record SendOpportunityResponsibleChanged(
    Guid EventId,
    string Source,
    Guid OpportunityId,
    string OpportunityTitle,
    string ResponsibleEmail,
    string ResponsibleFullname,
    string PreviousResponsibleFullname,
    string ChangedByFullname
) : IEmailTemplateContract;
