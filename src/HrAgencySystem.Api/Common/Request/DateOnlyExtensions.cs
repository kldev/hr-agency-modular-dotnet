namespace HrAgencySystem.Api.Common.Request;

public static class DateOnlyExtensions
{
    extension(DateOnly? date)
    {
        public DateTimeOffset? ToUtc(string timezone)
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
        public DateTimeOffset? ToUtc(string timezone)
        {

            var timeZone = GetTimeZone(timezone);
            var localDateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone));
        }
    }
}