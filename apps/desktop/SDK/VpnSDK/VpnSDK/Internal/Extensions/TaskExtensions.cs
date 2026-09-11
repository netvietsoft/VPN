using System.Threading.Tasks;

namespace VpnSDK.Internal.Extensions;

internal static class TaskExtensions
{
	public static void Forget(this Task task)
	{
		if (!task.IsCompleted || task.IsFaulted)
		{
			ForgetAwaited(task);
		}
	}

	public static async Task ForgetAwaited(Task task)
	{
		try
		{
			await task.ConfigureAwait(continueOnCapturedContext: false);
		}
		catch
		{
		}
	}
}
