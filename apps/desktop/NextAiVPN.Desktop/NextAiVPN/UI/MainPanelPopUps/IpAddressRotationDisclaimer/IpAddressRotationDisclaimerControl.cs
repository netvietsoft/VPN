using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.MainPanelPopUps.IpAddressRotationDisclaimer;

public partial class IpAddressRotationDisclaimerControl : UserControl, IComponentConnector
{
	public IpAddressRotationDisclaimerControlViewModel ViewModel { get; set; }

	public IpAddressRotationDisclaimerControl()
	{
		InitializeComponent();
	}
}
