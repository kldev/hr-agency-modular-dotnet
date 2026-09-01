namespace HrAgencySystem.IntegrationTests.Infrastructure;

public static class Eventually
{
    public static async Task AssertAsync(
        Func<Task> assertion,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        var timeoutValue = timeout ?? TimeSpan.FromSeconds(5);
        var intervalValue = interval ?? TimeSpan.FromMilliseconds(100);
        var deadline = DateTime.UtcNow + timeoutValue;

        Exception? lastException = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                await assertion();
                return;
            }
            catch (Exception exception)
            {
                lastException = exception;
            }

            await Task.Delay(intervalValue);
        }

        throw new TimeoutException(
            $"Assertion did not succeed within {timeoutValue}.",
            lastException);
    }
}
