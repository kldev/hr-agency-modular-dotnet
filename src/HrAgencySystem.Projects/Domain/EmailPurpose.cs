namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// What a project e-mail address is for. An enum rather than two fields so that a third purpose
/// costs a value, not a migration and a new API shape.
/// </summary>
public enum EmailPurpose
{
    Invoice,
    Document,
}
