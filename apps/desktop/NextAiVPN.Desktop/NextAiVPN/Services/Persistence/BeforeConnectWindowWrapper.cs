namespace NextAiVPN.Services.Persistence;

public class BeforeConnectWindowWrapper : IBeforeConnectWindowWrapper
{
	private readonly VPNWindowExpanded _vpnWindowExpanded;

	public BeforeConnectWindowWrapper(VPNWindowExpanded vpnWindowExpanded)
	{
		_vpnWindowExpanded = vpnWindowExpanded;
	}

	public void Show()
	{
		new BeforeConnectWindow(_vpnWindowExpanded).Show();
	}

	public void ShowDialog()
	{
		new BeforeConnectWindow(_vpnWindowExpanded).ShowDialog();
	}
}
