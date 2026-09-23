namespace HrAgencySystem.Forms.Documents;

/// <summary>
/// Holds a form code for one organization. The unique index on <c>(OrganizationId, Code)</c> is what
/// defeats two concurrent creates; the handler reads it first only to give a friendly answer.
/// </summary>
public sealed record FormCodeReservation(Guid Id, Guid OrganizationId, string Code, Guid FormId);
