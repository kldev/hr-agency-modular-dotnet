using System.Buffers.Text;
using System.Globalization;
using System.Text;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Recruitment.Application.Timeline.Queries;

/// <summary>
/// Where the previous page of a timeline ended: the last entry's time and sequence. Opaque to
/// the client, which only hands back what it was given.
/// </summary>
public sealed record TimelineCursor(DateTimeOffset OccurredAt, long Sequence)
{
    public const string InvalidCursorMessage = "The timeline cursor is not valid.";

    public string Encode()
    {
        var raw = string.Create(CultureInfo.InvariantCulture, $"{OccurredAt.UtcTicks}:{Sequence}");
        return Base64Url.EncodeToString(Encoding.UTF8.GetBytes(raw));
    }

    /// <summary>No cursor means the first page; a cursor that does not decode is a 400.</summary>
    public static TimelineCursor? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(value));
            var parts = raw.Split(':');

            if (
                parts.Length == 2
                && long.TryParse(parts[0], CultureInfo.InvariantCulture, out var ticks)
                && long.TryParse(parts[1], CultureInfo.InvariantCulture, out var sequence)
                && ticks >= DateTimeOffset.MinValue.UtcTicks
                && ticks <= DateTimeOffset.MaxValue.UtcTicks
            )
                return new TimelineCursor(new DateTimeOffset(ticks, TimeSpan.Zero), sequence);
        }
        catch (FormatException) { }

        throw new InValidValueException(InvalidCursorMessage);
    }
}
