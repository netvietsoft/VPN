using System;
using System.Threading;
using System.Threading.Tasks;

namespace VpnSDK.Internal.Helpers;

internal static class TaskWithCancellation
{
	public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
	{
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		using (cancellationToken.Register(delegate(object s)
		{
			((TaskCompletionSource<bool>)s).TrySetResult(result: true);
		}, taskCompletionSource))
		{
			if (task != await Task.WhenAny(new Task[2] { task, taskCompletionSource.Task }).ConfigureAwait(continueOnCapturedContext: false))
			{
				throw new OperationCanceledException(cancellationToken);
			}
		}
		return await task.ConfigureAwait(continueOnCapturedContext: false);
	}
}
