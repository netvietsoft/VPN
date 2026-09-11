using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class NextAiVpn_Error : Window, IComponentConnector
{
	public NextAiVpn_Error()
	{
		InitializeComponent();
		WindowHeader.GetParent(this);
	}

	private void btnDismiss_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
