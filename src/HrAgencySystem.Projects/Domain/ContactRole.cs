namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// One contact model with roles rather than a field per role: a third role would otherwise mean a
/// schema change. Four of these sit on the client's side of the table.
/// </summary>
public enum ContactRole
{
    /// <summary>Who answers for the project at the client. Required before a project goes live.</summary>
    Responsible,

    /// <summary>Who signed the contract. Not necessarily the person responsible today.</summary>
    ContractSignatory,

    Invoicing,

    OnSite,

    /// <summary>
    /// Ours, not the client's: the authorised recipient Germany requires (§ 18 AEntG) and the
    /// liaison person Belgium requires (art. 7/2 of the law of 5 March 2002). The same handful of
    /// details and the same "point somebody at this role" operation, so the same model.
    /// </summary>
    AuthorisedRecipient,
}
