namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// What a document about a person is. These travel with the person from project to project - a
/// passport, a diploma and a medical certificate do not care who the client is.
/// <para>
/// What is deliberately not here: the A1, the host country notification, the contract naming one
/// delivering company. Those are <see cref="AssignmentDocumentCategory"/>, because they are true of
/// one posting and not of the person.
/// </para>
/// </summary>
public enum WorkerDocumentCategory
{
    Identity,
    EmploymentContract,
    MedicalCertificate,
    HealthAndSafety,
    Qualification,

    /// <summary>Residence card, work permit, visa - the proof behind a <see cref="WorkAuthorisation"/>.</summary>
    Legalisation,
    Other,
}
