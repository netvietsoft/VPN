namespace System.Threading.Tasks;

internal static class RetryOperationHelper
{
	public static async Task<T> ExecuteWithRetry<T>(Func<Task<T>> func, int maxAttempts, TimeSpan? retryInterval = null, Action<int, Exception> onAttemptFailed = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		if (maxAttempts < 1)
		{
			throw new ArgumentOutOfRangeException("maxAttempts", maxAttempts, "The maximum number of attempts must not be less than 1.");
		}
		int attempt = 0;
		while (true)
		{
			if (attempt > 0 && retryInterval.HasValue)
			{
				await Task.Delay(retryInterval.Value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			try
			{
				return await func().ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception arg)
			{
				attempt++;
				onAttemptFailed?.Invoke(attempt, arg);
				if (attempt >= maxAttempts)
				{
					throw;
				}
			}
		}
	}

	public static async Task ExecuteWithRetry(Func<Task> func, int maxAttempts, TimeSpan? retryInterval = null, Action<int, Exception> onAttemptFailed = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		await ExecuteWithRetry(async delegate
		{
			await func().ConfigureAwait(continueOnCapturedContext: false);
			return true;
		}, maxAttempts, retryInterval, onAttemptFailed, cancellationToken);
	}
}
