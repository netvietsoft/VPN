using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class BeforeConnectWindow : Window, IComponentConnector
{
	private readonly VPNWindowExpanded _expandedVpnWindow;

	private bool _isIUnderstandCommited;

	public BeforeConnectWindow(VPNWindowExpanded expanded)
	{
		InitializeComponent();
		_expandedVpnWindow = expanded;
		_expandedVpnWindow.IsBeforeConnectVisible = true;
	}

	private void ExitApplication()
	{
		_isIUnderstandCommited = true;
		SaveIUnderstand();
		_expandedVpnWindow.IsBeforeConnectVisible = false;
		this.Hide();
		_expandedVpnWindow.Show();
	}

	private void IUnderstand_Click(object sender, RoutedEventArgs e)
	{
		_isIUnderstandCommited = true;
		_expandedVpnWindow.SdkObject.TaskBarService?.ContextMenuShow(string.Empty);
		SaveIUnderstand();
		Close();
	}

	private void SaveIUnderstand()
	{
		try
		{
			Utils.AppSettingsHelper.SetValue("IUnderstand", "1");
			Utils.PreferencesRepository.SaveSinglePreference("iunderstand", "1");
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error("Error saving I Understand confirmation: " + ex.Message, "SaveIUnderstand", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\BeforeConnectWindow.xaml.cs", 49);
		}
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		try
		{
			_isIUnderstandCommited = true;
			SaveIUnderstand();
			if (_expandedVpnWindow != null)
			{
				_expandedVpnWindow.IsBeforeConnectVisible = false;
				if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.HasShutdownStarted && !_expandedVpnWindow.AllowClose)
				{
					try
					{
						_expandedVpnWindow.Show();
					}
					catch { }
				}
			}
		}
		catch { }
	}

	private void BeforeConnectWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		_expandedVpnWindow.SdkObject.TaskBarService.TaskbarIcon.Visibility = Visibility.Visible;
		_expandedVpnWindow.SdkObject.TaskBarService.ContextMenuHide("Quit");
		WindowHeader.GetParent(this);
	}
}
