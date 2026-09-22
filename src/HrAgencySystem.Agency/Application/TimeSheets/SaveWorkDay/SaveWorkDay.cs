namespace HrAgencySystem.Agency.Application.TimeSheets.SaveWorkDay;

/// <summary>
/// Writing a day is what brings a sheet into being, so this command carries the month rather than
/// assuming one exists.
/// </summary>
public sealed record SaveWorkDay(
    Guid OrganizationId,
    Guid UserId,
    int Year,
    int Month,
    DateOnly Date,
    TimeOnly StartsAt,
    int Hours,
    int Minutes,
    string? Note,
    Guid ModifiedBy
);
