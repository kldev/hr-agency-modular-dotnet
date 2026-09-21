namespace HrAgencySystem.SharedKernel.ValueObjects;

/// <summary>
/// What we sign with the person working on the position, which is a different question from the
/// compliance engagement type - that one says on what basis we serve the client.
/// One position can carry both: a painter on an employment contract and a painter on a mandate
/// contract are two positions precisely because the paperwork differs.
/// <para>
/// Deliberately not <c>SharedKernel.EmploymentType</c>: that enum mixes the size of the job
/// (full time, part time) with its kind and was written for a job advert, not for a document.
/// </para>
/// </summary>
public enum WorkerContractType
{
    /// <summary>Umowa o pracę.</summary>
    EmploymentContract,

    /// <summary>Umowa o pracę tymczasową.</summary>
    TemporaryEmploymentContract,

    /// <summary>Umowa zlecenie.</summary>
    MandateContract,

    /// <summary>Kontrakt B2B - the person invoices us.</summary>
    SelfEmployed,

    Other,
}
