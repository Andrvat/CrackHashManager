public static class RetryHelper
{
    public static async Task RetryOnExceptionAsync(int times, TimeSpan delay, Func<Task> operation)
    {
        await RetryOnExceptionAsync<Exception>(times, delay, operation);
    }

    private static async Task RetryOnExceptionAsync<TException>(int times, TimeSpan delay, Func<Task> operation) 
        where TException : Exception
    {
        if (times <= 0)
            throw new ArgumentOutOfRangeException(nameof(times));

        var attempts = 0;
        do
        {
            try
            {
                attempts++;
                Console.WriteLine($"Attempt number: {attempts}");
                await operation();
                break;
            }
            catch (TException ex)
            {
                if (attempts == times)
                {
                    throw;
                }

                await Task.Delay(delay);
            }
        } while (true);
    }
}