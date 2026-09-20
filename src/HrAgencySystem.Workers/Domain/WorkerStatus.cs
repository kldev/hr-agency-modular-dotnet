namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// Where a person stands with us. This is a pipeline, not a label, and each stage is somebody else's
/// work: recruitment hands over a signed candidate, HR prepares the contract and the paperwork,
/// legalisation obtains the right to be here and to work, operations put the person to work and
/// house them. Reading the stage answers "whose desk is this on", which is the question actually
/// being asked when somebody opens the list.
/// <para>
/// Not the same thing as an assignment's status. Somebody between two postings is still
/// <see cref="Employed"/>; somebody whose residence card has not arrived is still in
/// <see cref="Legalisation"/> even though a project is waiting for them.
/// </para>
/// </summary>
public enum WorkerStatus
{
    /// <summary>Being recruited. Ends where recruitment ends: the candidate signs with us.</summary>
    Recruitment,

    /// <summary>Contract and employment paperwork being prepared. HR.</summary>
    ContractPreparation,

    /// <summary>
    /// Residence card, PESEL, bank account, work permit. Only for people who need them - see
    /// <see cref="LegalisationPolicy"/>. A national of an EEA state passes this stage by.
    /// </summary>
    Legalisation,

    /// <summary>Being put to work: introduction, accommodation, the practical side. Operations.</summary>
    Onboarding,

    /// <summary>Working for us.</summary>
    Employed,

    /// <summary>
    /// Moving from one project to another. A stage of its own rather than a gap between two
    /// assignments, because it is work somebody has to do: the person already has their papers, but
    /// a new posting entity means a new A1, a new country means a new notification, and a new site
    /// usually means somewhere else to live. Whoever moves them needs to see that on a list.
    /// </summary>
    ProjectChange,

    /// <summary>
    /// Gone. Left, was let go, or never arrived at all. The file stays, because the history on it
    /// has to - and because the same person coming back next season is the same person.
    /// </summary>
    Terminated,
}
