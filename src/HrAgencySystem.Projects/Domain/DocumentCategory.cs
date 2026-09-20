namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// What a project document is. Open ended on purpose: a value costs nothing, and the alternative -
/// free text - makes "show me every expiring insurance policy" impossible to ask.
/// </summary>
public enum DocumentCategory
{
    Contract,
    Annex,
    Invoice,
    ClientDocument,
    Compliance,
    Insurance,
    Other,
}
