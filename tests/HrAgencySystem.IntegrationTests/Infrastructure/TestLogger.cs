using Microsoft.Extensions.Logging;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public sealed class TestLogger(
    string categoryName,
    TestLoggerProvider provider) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        provider.Write(
            logLevel,
            categoryName,
            formatter(state, exception),
            exception);
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}