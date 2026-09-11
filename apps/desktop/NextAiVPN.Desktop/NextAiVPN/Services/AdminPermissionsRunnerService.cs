using System;
using System.Security.Principal;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.MessageBoxWindows.RunAsAdmin;

namespace NextAiVPN.Services;

internal class AdminPermissionsRunnerService : IAdminPermissionsRunnerService
{
	private readonly IAdminRunner _adminRunner;

	private readonly IRoleChecker _roleChecker;

	private readonly VPNWindowExpanded _windowExpanded;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public AdminPermissionsRunnerService(IAdminRunner adminRunner, IRoleChecker roleChecker, VPNWindowExpanded windowExpanded, IAppSettingsHelper appSettingsHelper)
	{
		_adminRunner = adminRunner;
		_roleChecker = roleChecker;
		_windowExpanded = windowExpanded;
		_appSettingsHelper = appSettingsHelper;
	}

	public bool TryToRunOnStartUp()
	{
		if (_roleChecker.IsInRole(WindowsBuiltInRole.Administrator))
		{
			return true;
		}
		RunAsAdminMessageBoxWindow runAsAdminMessageBoxWindow = new RunAsAdminMessageBoxWindow(new RunAsAdminMessageBoxWindowViewModel());
		runAsAdminMessageBoxWindow.ShowDialog();
		if (runAsAdminMessageBoxWindow.DialogResult)
		{
			_adminRunner.RunAsAdmin();
		}
		return false;
	}

	public bool TryToRun(RunAsAdminOption option)
	{
		if (_roleChecker.IsInRole(WindowsBuiltInRole.Administrator))
		{
			return true;
		}
		RunAsAdminMessageBoxWindow runAsAdminMessageBoxWindow = new RunAsAdminMessageBoxWindow(new RunAsAdminMessageBoxWindowViewModel());
		runAsAdminMessageBoxWindow.ShowDialog();
		if (runAsAdminMessageBoxWindow.DialogResult)
		{
			switch (option)
			{
			case RunAsAdminOption.KillSwitch:
				_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Toggled1 = true;
				_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SetKillSwitch();
				break;
			case RunAsAdminOption.OpenVpn:
				_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.SetProtocol(VpnProtocol.OpenVPN);
				_appSettingsHelper.SetValue("OpenTab", "4");
				break;
			default:
				throw new ArgumentOutOfRangeException("option", option, null);
			case RunAsAdminOption.TapDriver:
				break;
			}
			_adminRunner.RunAsAdmin();
			return false;
		}
		switch (option)
		{
		case RunAsAdminOption.KillSwitch:
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Toggled1 = false;
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SetKillSwitch();
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Dot_InitializeState();
			break;
		case RunAsAdminOption.OpenVpn:
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.IKEv2.IsChecked = true;
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.OpenVPN.IsChecked = false;
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.SetProtocol(VpnProtocol.IKEv2);
			break;
		default:
			throw new ArgumentOutOfRangeException("option", option, null);
		case RunAsAdminOption.TapDriver:
			break;
		}
		return false;
	}
}
