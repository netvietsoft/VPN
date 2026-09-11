using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class SubscriptionValidationError : Window, IComponentConnector
{
	public SubscriptionValidationError()
	{
		InitializeComponent();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void SubscriptionValidationError_OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}
}
