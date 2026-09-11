using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class NoNetwork_Control : UserControl, IComponentConnector
{
	private VPNWindowExpanded _vpnexpandedmain;

	private NoNetwork _nonetworkwindow;

	public NoNetwork_Control()
	{
		InitializeComponent();
	}

	public void GetMainWindow(NoNetwork nonetwork, VPNWindowExpanded expanded)
	{
		_nonetworkwindow = nonetwork;
		_vpnexpandedmain = expanded;
	}

	private void ExpandCollapse_MouseEnter(object sender, MouseEventArgs e)
	{
		ExpandCollapse.Opacity = 1.0;
	}

	private void ExpandCollapse_MouseLeave(object sender, MouseEventArgs e)
	{
		ExpandCollapse.Opacity = 0.72;
	}

	private void ExpandCollapse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		try
		{
			_nonetworkwindow.Show();
			_vpnexpandedmain.Hide();
		}
		catch (Exception)
		{
		}
	}
}
