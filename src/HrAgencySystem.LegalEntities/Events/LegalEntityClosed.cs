using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.LegalEntities.Events;

/// <summary>
/// The entity stopped trading. Its own fact rather than an edit of a date field, because "this
/// company was wound up on the 30th" is something anyone reading the history should see happen,
/// not something to infer from a value that quietly changed.
/// </summary>
public sealed record LegalEntityClosed(
    Guid LegalEntityId,
    Guid OrganizationId,
    DateOnly ActiveTo,
    UserSnapshot ClosedBy,
    DateTimeOffset ClosedAt
);
