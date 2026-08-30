namespace HrAgencySystem.SharedKernel.Time;

public sealed class FixedClock(DateTimeOffset value) : IClock
{
    public DateTimeOffset UtcNow => value.ToUniversalTime();
}