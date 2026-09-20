namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// Who owns a person at a given stage. Kept as its own concept rather than inferred from the status
/// at every call site, because the mapping is a fact about how the agency is organised and it is the
/// thing a work queue is filtered by.
/// </summary>
public enum ResponsibleDepartment
{
    Recruitment,
    HumanResources,
    Legalisation,
    Operations,

    /// <summary>Nobody: the file has stopped moving.</summary>
    None,
}
