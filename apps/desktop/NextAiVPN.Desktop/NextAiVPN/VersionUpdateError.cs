using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class VersionUpdateError : Window, IComponentConnector
{
	public VersionUpdateError()
	{
		InitializeComponent();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	public void GetErrorMessage(string error)
	{
		Description.Text = error;
	}

	private void VersionUpdateError_OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}
}
