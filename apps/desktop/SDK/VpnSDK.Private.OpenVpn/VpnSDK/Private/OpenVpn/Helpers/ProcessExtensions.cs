using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VpnSDK.Private.OpenVpn.Helpers;

public static class ProcessExtensions
{
	public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		process.EnableRaisingEvents = true;
		process.Exited += Process_Exited;
		try
		{
			if (!process.HasExited)
			{
				using (cancellationToken.Register(delegate
				{
					tcs.TrySetCanceled();
				}))
				{
					await tcs.Task.ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
		finally
		{
			process.Exited -= Process_Exited;
		}
		void Process_Exited(object sender, EventArgs e)
		{
			tcs.TrySetResult(result: true);
		}
	}
}
