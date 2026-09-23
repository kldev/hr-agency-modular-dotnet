namespace HrAgencySystem.Forms.Domain;

/// <summary>
/// A label, not a mechanism. A document (consent, statement, tax form) and a survey (satisfaction,
/// exit interview) are the same model; the kind only picks sensible defaults and lets the lists be
/// filtered. Nothing in the module asks <c>if (Kind == Survey)</c>.
/// <para>Append only: Marten stores the ordinal.</para>
/// </summary>
public enum FormKind
{
    Document,
    Survey,
}

/// <summary>
/// How many responses one person may have to this form. Fixed when the form is created: no command
/// changes it, because turning "many" into "one" would leave people with several of what is now
/// supposed to be unique.
/// </summary>
public enum ResponseCardinality
{
    /// <summary>A consent, a statement - one per person, across every version of the form.</summary>
    OnePerSubject,

    /// <summary>A survey after every project - as many as there are occasions.</summary>
    Many,
}

/// <summary>Where a form stands. Worked out from the stream, never stored as its own fact.</summary>
public enum FormStatus
{
    /// <summary>Never published; nobody can fill it in yet.</summary>
    Draft,

    /// <summary>At least one version is out. The draft may already be ahead of it.</summary>
    Published,

    /// <summary>Closed to new responses. Everything already given stays readable.</summary>
    Archived,
}

public enum FormResponseStatus
{
    Draft,
    Submitted,
}
