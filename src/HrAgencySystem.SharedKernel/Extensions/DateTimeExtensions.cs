namespace HrAgencySystem.SharedKernel.Extensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset ToInstantUtc(this DateTime dateTime, string dateTimezone)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(dateTimezone);

        var utc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified),
            timezone
        );

        return new DateTimeOffset(utc);
    }
}
