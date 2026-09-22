namespace HrAgencySystem.Agency.Domain.TimeSheets;

/// <summary>
/// One day of work: when it began and how long it lasted.
/// <para>
/// The end is not stored, it is arithmetic. People think in "I came in at eight and worked eight
/// and a half", not in "eight to half four", and asking for the end makes them do the sum in their
/// head before they can type. A day that runs past midnight is allowed and still belongs to the day
/// it started on.
/// </para>
/// </summary>
public sealed record WorkDay(DateOnly Date, TimeOnly StartsAt, int Minutes, string Note)
{
    /// <summary>When it ended, which may be on the next day.</summary>
    public DateTime EndsAt => Date.ToDateTime(StartsAt).AddMinutes(Minutes);

    public bool CrossesMidnight => EndsAt.Date > Date.ToDateTime(TimeOnly.MinValue).Date;
}
