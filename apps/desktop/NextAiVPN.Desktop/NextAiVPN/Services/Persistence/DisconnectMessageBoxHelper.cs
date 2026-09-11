using NextAiVPN.UI.MessageBoxWindows;

namespace NextAiVPN.Services.Persistence;

internal class DisconnectMessageBoxHelper : IDisconnectMessageBoxHelper
{
	private readonly SDKMonitor _sdk;

	public DisconnectMessageBoxHelper(SDKMonitor sdk)
	{
		_sdk = sdk;
	}

	public bool ShowDisconnectMessageIfConnected(bool isStreaming = false)
	{
		if (isStreaming)
		{
			if (!_sdk.StreamingSdk.IsConnected)
			{
				return false;
			}
			if (!_sdk.StreamingSdk.IsActive)
			{
				return false;
			}
		}
		else
		{
			if (!_sdk.NextAiVpnSdkManager.IsConnected)
			{
				return false;
			}
			if (_sdk.NextAiVpnSdkManager.IsConnecting)
			{
				return false;
			}
		}
		MessageBoxWindow messageBoxWindow = new MessageBoxWindow(new MessageBoxWindowViewModel
		{
			Header = "Disconnect",
			Description = "It’s necessary to disconnect from the VPN before modifying settings.",
			CancelButtonText = "Cancel",
			OkButtonText = "Disconnect"
		});
		messageBoxWindow.ShowDialog();
		if (messageBoxWindow.DialogResult)
		{
			_sdk.DisconnectVPN();
		}
		return !messageBoxWindow.DialogResult;
	}
}
