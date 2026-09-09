using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Infrastructure;

public sealed class TestLoggerProvider : ILoggerProvider
{
    private ITestOutputHelper? _output;

    public void SetOutput(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
        => new TestLogger(categoryName, this);

    internal void Write(
        LogLevel logLevel,
        string categoryName,
        string message,
        Exception? exception)
    {
        _output?.WriteLine(
            $"[{DateTimeOffset.Now:HH:mm:ss.fff}] " +
            $"[{logLevel}] " +
            $"[{categoryName}] " +
            message);

        if (exception is not null)
            _output?.WriteLine(exception.ToString());
    }

    public void Dispose()
    {
    }
}