using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Preferences;

public partial class PreferencesControl : UserControl, IComponentConnector
{
	public PreferencesControl()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		StartUpToggle.Dot_InitializeState();
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += StyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= StyleChanged;
	}
}
