using System;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class ExpandedProtocolSettings : UserControl, IComponentConnector
{
	private SDKMonitor _sdkObject;

	private IRoleChecker _roleChecker;

	private IDisconnectMessageBoxHelper _disconnectMessageBoxHelper;

	public ExpandedProtocolSettings()
	{
		InitializeComponent();
	}

	private void OnVpnModeChanged(VpnType obj)
	{
		_sdkObject.VpnExpandedWindow.PreferencesService.RestoreProtocol();
		switch (obj)
		{
		case VpnType.NextAiVPN:
			SetPrivateMode();
			break;
		case VpnType.Streaming:
			SetStreamingMode();
			break;
		default:
			throw new ArgumentOutOfRangeException("obj", obj, null);
		}
	}

	private void SetStreamingMode()
	{
		OtherProtocolsPanel.IsEnabled = false;
		OtherProtocolsPanel.Opacity = 0.4;
		OpenVPNOptions.Visibility = Visibility.Collapsed;
	}

	private void SetPrivateMode()
	{
		OtherProtocolsPanel.IsEnabled = true;
		OtherProtocolsPanel.Opacity = 1.0;
		OpenVPNOptions.Visibility = Visibility.Visible;
		SetOpenVpnOptionsAvailability(OpenVPNOptions.IsEnabled);
	}

	public void SetProtocolEnable(bool value)
	{
	}

	public void SetDependencies(SDKMonitor sdkMonitor, IDisconnectMessageBoxHelper disconnectMessageBoxHelper, IRoleChecker roleChecker)
	{
		_sdkObject = sdkMonitor;
		_disconnectMessageBoxHelper = disconnectMessageBoxHelper;
		_roleChecker = roleChecker;
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
	}

	public void GetParents(SDKMonitor sdk)
	{
		_sdkObject = sdk;
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
	}

	private void IKEv2_Checked(object sender, RoutedEventArgs e)
	{
		if (_sdkObject.GetVpnMode() != VpnType.Streaming)
		{
			if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
			{
				BackSelectedProtocol();
			}
			else
			{
				SetIkEv2();
			}
		}
	}

	private void SetIkEv2()
	{
		Utils.VpnConfigurationSaver.SaveProtocol("IKEv2");
		Utils.VpnConfigurationSaver.SaveOpenVpnType("");
		Utils.VpnConfigurationSaver.SaveScrambleConfiguration(status: false);
		_sdkObject.SetProtocol(1, null);
		SetOpenVpnOptionsAvailability(isAvailable: false);
		scramble.IsChecked = false;
		Utils.MixpanelNotification.SendNotification("Settings - IKEv2");
	}

	private void BackSelectedProtocol()
	{
		string value = Utils.AppSettingsHelper.GetValue("protocol");
		if (value != null)
		{
			if (value.Equals("wireguard", StringComparison.OrdinalIgnoreCase))
			{
				WireGuard.Checked -= WireGuard_OnChecked;
				WireGuard.IsChecked = true;
				WireGuard.Checked += WireGuard_OnChecked;
			}
			else if (value.Equals("openvpn", StringComparison.OrdinalIgnoreCase))
			{
				OpenVPN.Checked -= OpenVPN_Checked;
				OpenVPN.IsChecked = true;
				OpenVPN.Checked += OpenVPN_Checked;
			}
			else if (value.Equals("ikev2", StringComparison.OrdinalIgnoreCase))
			{
				IKEv2.Checked -= IKEv2_Checked;
				IKEv2.IsChecked = true;
				IKEv2.Checked += IKEv2_Checked;
			}
		}
	}

	private void BackSelectedOpenVpnProtocol()
	{
		string value = Utils.AppSettingsHelper.GetValue("protocolType");
		if (value != null)
		{
			if (value.Equals("udp", StringComparison.OrdinalIgnoreCase))
			{
				UDP.Checked += UDP_Checked;
				UDP.IsChecked = true;
				UDP.Checked += UDP_Checked;
			}
			else if (value.Equals("tcp", StringComparison.OrdinalIgnoreCase))
			{
				TCP.Checked -= TCP_Checked;
				TCP.IsChecked = true;
				TCP.Checked += TCP_Checked;
			}
		}
	}

	private void OpenVPN_Checked(object sender, RoutedEventArgs e)
	{
		if (_sdkObject.GetVpnMode() == VpnType.Streaming)
		{
			return;
		}
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			BackSelectedProtocol();
			return;
		}
		try
		{
			if (!_sdkObject.VpnExpandedWindow.AdminPermissionsRunnerService.TryToRun(RunAsAdminOption.OpenVpn))
			{
				return;
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "OpenVPN_Checked", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\ExpandedProtocolSettings.xaml.cs", 171);
		}
		SetOpenVpn();
	}

	public void SetProtocol(VpnProtocol vpnProtocol)
	{
		switch (vpnProtocol)
		{
		case VpnProtocol.IKEv2:
			SetIkEv2();
			break;
		case VpnProtocol.OpenVPN:
			SetOpenVpn();
			break;
		default:
			throw new ArgumentOutOfRangeException("vpnProtocol", vpnProtocol, null);
		}
	}

	private void SetOpenVpn()
	{
		Utils.VpnConfigurationSaver.SaveProtocol("OpenVPN");
		SetOpenVpnOptionsAvailability(isAvailable: true);
	}

	private void TCP_Checked(object sender, RoutedEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			BackSelectedOpenVpnProtocol();
		}
		else if (_roleChecker.IsInRole(WindowsBuiltInRole.Administrator))
		{
			if (!_sdkObject.CheckTapDriverInstalled())
			{
				IKEv2.IsChecked = true;
				return;
			}
			Utils.VpnConfigurationSaver.SaveOpenVpnType("TCP");
			_sdkObject.SetProtocol(2, "tcp");
			Utils.MixpanelNotification.SendNotification("Settings - TCP");
		}
	}

	private void UDP_Checked(object sender, RoutedEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			BackSelectedOpenVpnProtocol();
		}
		else if (_roleChecker.IsInRole(WindowsBuiltInRole.Administrator))
		{
			if (!_sdkObject.CheckTapDriverInstalled())
			{
				IKEv2.IsChecked = true;
				return;
			}
			Utils.VpnConfigurationSaver.SaveOpenVpnType("UDP");
			_sdkObject.SetProtocol(2, "udp");
			Utils.MixpanelNotification.SendNotification("Settings - UDP");
		}
	}

	private void Scramble_Click(object sender, RoutedEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			scramble.IsChecked = Utils.AppSettingsHelper.GetValue("scramble").Equals("1");
		}
		else if (scramble.IsChecked.ToString().Equals("True"))
		{
			Utils.VpnConfigurationSaver.SaveScrambleConfiguration(status: true);
			_sdkObject.SetScramble(flag: true);
			Utils.MixpanelNotification.SendNotification("Settings - Scramble_ON");
		}
		else
		{
			Utils.VpnConfigurationSaver.SaveScrambleConfiguration(status: false);
			_sdkObject.SetScramble(flag: false);
			Utils.MixpanelNotification.SendNotification("Settings - Scramble_OFF");
		}
	}

	private void InstallDriver_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (!_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			InstallMessage installMessage = new InstallMessage(_sdkObject.TapDriverInstaller, _sdkObject.BrowserLinksOpener);
			base.Opacity = 0.5;
			installMessage.ShowDialog();
			base.Opacity = 1.0;
		}
	}

	private void WireGuard_OnChecked(object sender, RoutedEventArgs e)
	{
		if (_sdkObject.GetVpnMode() != VpnType.Streaming)
		{
			if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
			{
				BackSelectedProtocol();
				return;
			}
			Utils.VpnConfigurationSaver.SaveProtocol("WireGuard");
			Utils.VpnConfigurationSaver.SaveOpenVpnType("");
			Utils.VpnConfigurationSaver.SaveScrambleConfiguration(status: false);
			_sdkObject.SetProtocol(3, null);
			SetOpenVpnOptionsAvailability(isAvailable: false);
			scramble.IsChecked = false;
			Utils.MixpanelNotification.SendNotification("Settings - WireGuard");
		}
	}

	public void SetOpenVpnOptionsAvailability(bool isAvailable)
	{
		if (isAvailable)
		{
			// [VI] Bỏ qua kiểm tra TAP Driver gây khóa UI / [EN] Bypass blocking TAP driver modal
			OpenVPNOptions.IsEnabled = true;
			OpenVPNOptions.Opacity = 1.0;
			if (string.IsNullOrEmpty(Utils.AppSettingsHelper.GetValue("protocolType")) || Utils.AppSettingsHelper.GetValue("protocolType").ToUpper().Equals("TCP"))
			{
				TCP.IsChecked = true;
			}
			else
			{
				UDP.IsChecked = true;
			}
			OpenVPNOptions.Visibility = Visibility.Visible;
		}
		else
		{
			OpenVPNOptions.IsEnabled = false;
			OpenVPNOptions.Opacity = 0.4;
			OpenVPNOptions.Visibility = Visibility.Collapsed;
			TCP.IsChecked = false;
			UDP.IsChecked = false;
		}
	}
}
