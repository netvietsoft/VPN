using System;
using System.Linq;
using System.Threading.Tasks;
using DotRas;

namespace VpnSDK.Private.Ras.Utilities;

public static class RasExistingConnection
{
	public static async Task DisconnectAsync(string connectionName, bool exactMatch = false)
	{
		DotRas.RasConnection rasConnection = DotRas.RasConnection.GetActiveConnections().FirstOrDefault((DotRas.RasConnection x) => (!exactMatch) ? x.EntryName.StartsWith(connectionName) : x.EntryName.Equals(connectionName, StringComparison.Ordinal));
		if (rasConnection != null)
		{
			try
			{
				await rasConnection.HangUpAsync(closeAllReferences: true).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch
			{
			}
		}
	}

	public static void Disconnect(string connectionName, bool exactMatch = false, int timeout = 500)
	{
		try
		{
			DisconnectAsync(connectionName, exactMatch).Wait(timeout);
		}
		catch
		{
		}
	}
}
