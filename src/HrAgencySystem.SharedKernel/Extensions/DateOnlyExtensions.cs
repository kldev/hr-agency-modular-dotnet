namespace HrAgencySystem.SharedKernel.Extensions;

public static class DateOnlyExtensions
{
    extension(DateOnly? date)
    {
        public DateTimeOffset? ToUtc(string timezone = "Europe/Warsaw")
        {
            if (date == null) return null;

            var timeZone = GetTimeZone(timezone);
            var localDateTime = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone));
        }

        private static TimeZoneInfo GetTimeZone(string timezone)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new ArgumentException($"Unknown timezone '{timezone}'.");
            }
            catch (InvalidTimeZoneException)
            {
                throw new ArgumentException($"Invalid timezone '{timezone}'.");
            }
        }
    }

    extension(DateOnly date)
    {
        public DateTimeOffset? ToUtc(string timezone= "Europe/Warsaw")
        {

            var timeZone = GetTimeZone(timezone);
            var localDateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone));
        }
    }

    extension(DateTime date)
    {
        public DateOnly ToDateOnly()
            => new (date.Year, date.Month, date.Day);
    }
}