using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.UnitTests;

public abstract class BaseTest
{
    protected IClock TestClock { get; } = new FixedClock(DateTime.UtcNow);
}