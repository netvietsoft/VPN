using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class ExpandedSideMenu : UserControl, IComponentConnector
{
	private VPNWindowExpanded _windowExpanded;
	private SideMenuOption _selectedSettings = SideMenuOption.Dashboard;

	public Action<SideMenuOption> OnTabChanged;

	private readonly SolidColorBrush _activeBg = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#DCFCE7"));
	private readonly SolidColorBrush _activeFg = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#047857"));
	private readonly SolidColorBrush _activeIconFg = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#059669"));
	private readonly SolidColorBrush _hoverBg = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0FDF4"));
	private readonly SolidColorBrush _normalFg = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#334155"));
	private readonly SolidColorBrush _normalIconFg = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#64748B"));
	private readonly SolidColorBrush _transparentBg = Brushes.Transparent;

	public ExpandedSideMenu()
	{
		InitializeComponent();
	}

	public void SetMenuOption(SideMenuOption option)
	{
		_selectedSettings = option;
		OnTabChanged?.Invoke(option);

		ResetAllTabsVisual();

		switch (option)
		{
			case SideMenuOption.Dashboard:
				HighlightTab(DashboardSideMenu, DashboardTitle, DashboardIcon);
				break;
			case SideMenuOption.Location:
				HighlightTab(LocationsSideMenu, LocationsTitle, LocationsIcon);
				break;
			case SideMenuOption.ResidentialMesh:
				HighlightTab(ResidentialMeshSideMenu, ResidentialMeshTitle, ResidentialMeshIcon);
				break;
			case SideMenuOption.SpeedTest:
				HighlightTab(SpeedTestSideMenu, SpeedTestTitle, SpeedTestIcon);
				break;
			case SideMenuOption.Protocol:
				HighlightTab(ProtocolSideMenu, SettingsProtocolOption, ProtocolIcon);
				break;
			case SideMenuOption.Security:
				HighlightTab(SecuritySideMenu, SecurityTitle, SecurityIcon);
				break;
			case SideMenuOption.Account:
				HighlightTab(AccountSideMenu, AccountTextBlock, AccountIcon);
				break;
			case SideMenuOption.Settings:
				HighlightTab(GeneralSideMenu, SettingsGeneralOption, GeneralIcon);
				break;
			case SideMenuOption.Notifications:
				HighlightTab(NotificationsMenu, NotificationsTextBlock, NotificationsIcon);
				break;
			case SideMenuOption.GetHelp:
			case SideMenuOption.LegalInformation:
				HighlightTab(GetHelpMenu, GetHelpTextBlock, GetHelpIcon);
				break;
		}

		SetSelectedMenuOption(option);
	}

	private void ResetAllTabsVisual()
	{
		ResetTab(DashboardSideMenu, DashboardTitle, DashboardIcon);
		ResetTab(LocationsSideMenu, LocationsTitle, LocationsIcon);
		ResetTab(ResidentialMeshSideMenu, ResidentialMeshTitle, ResidentialMeshIcon);
		ResetTab(SpeedTestSideMenu, SpeedTestTitle, SpeedTestIcon);
		ResetTab(ProtocolSideMenu, SettingsProtocolOption, ProtocolIcon);
		ResetTab(SecuritySideMenu, SecurityTitle, SecurityIcon);
		ResetTab(AccountSideMenu, AccountTextBlock, AccountIcon);
		ResetTab(GeneralSideMenu, SettingsGeneralOption, GeneralIcon);
		ResetTab(NotificationsMenu, NotificationsTextBlock, NotificationsIcon);
		ResetTab(GetHelpMenu, GetHelpTextBlock, GetHelpIcon);
	}

	private void ResetTab(Border border, TextBlock textBlock, System.Windows.Shapes.Path iconPath = null)
	{
		if (border == null || textBlock == null) return;
		border.Background = _transparentBg;
		textBlock.Foreground = _normalFg;
		textBlock.FontWeight = FontWeights.Normal;
		if (iconPath != null)
		{
			iconPath.Fill = _normalIconFg;
		}
	}

	private void HighlightTab(Border border, TextBlock textBlock, System.Windows.Shapes.Path iconPath = null)
	{
		if (border == null || textBlock == null) return;
		border.Background = _activeBg;
		textBlock.Foreground = _activeFg;
		textBlock.FontWeight = FontWeights.Bold;
		if (iconPath != null)
		{
			iconPath.Fill = _activeIconFg;
		}
	}

	public void SetSelectedMenuOption(SideMenuOption option)
	{
		if (_windowExpanded == null) return;

		_windowExpanded.Mainpanel.Visibility = Visibility.Collapsed;
		_windowExpanded.ExpandedLocations.Visibility = Visibility.Collapsed;
		_windowExpanded.ResidentialMeshControl.Visibility = Visibility.Collapsed;
		_windowExpanded.SpeedTestControl.Visibility = Visibility.Collapsed;
		_windowExpanded.AccountControl.Visibility = Visibility.Collapsed;
		_windowExpanded.GeneralControl.Visibility = Visibility.Collapsed;
		_windowExpanded.ProtocolsControl.Visibility = Visibility.Collapsed;
		_windowExpanded.AdvanceViewLogs.Visibility = Visibility.Collapsed;
		_windowExpanded.NotificationCenter.Visibility = Visibility.Collapsed;
		_windowExpanded.GetHelpControl.Visibility = Visibility.Collapsed;
		_windowExpanded.TermsAndPoliciesControl.Visibility = Visibility.Collapsed;
		_windowExpanded.Preferences.Visibility = Visibility.Collapsed;

		switch (option)
		{
			case SideMenuOption.Dashboard:
				_windowExpanded.Mainpanel.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.Location:
				_windowExpanded.ExpandedLocations.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.ResidentialMesh:
				_windowExpanded.ResidentialMeshControl.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.SpeedTest:
				_windowExpanded.SpeedTestControl.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.Protocol:
				_windowExpanded.ProtocolsControl.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.Security:
			case SideMenuOption.Settings:
				_windowExpanded.GeneralControl.Visibility = Visibility.Visible;
				if (_windowExpanded.SettingsControlViewModel != null)
				{
					_windowExpanded.SettingsControlViewModel.ControlVisibility = Visibility.Visible;
				}
				break;
			case SideMenuOption.Account:
				_windowExpanded.AccountControl.Visibility = Visibility.Visible;
				ThreadPool.QueueUserWorkItem(delegate
				{
					try
					{
						_windowExpanded?.AccountViewModel?.RefreshData();
					}
					catch { }
				});
				break;
			case SideMenuOption.Notifications:
				_windowExpanded.NotificationCenter.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.LogDetails:
				_windowExpanded.AdvanceViewLogs.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.GetHelp:
				_windowExpanded.GetHelpControl.Visibility = Visibility.Visible;
				break;
			case SideMenuOption.LegalInformation:
				_windowExpanded.TermsAndPoliciesControl.Visibility = Visibility.Visible;
				break;
		}
	}

	public void GetExpandedWindow(VPNWindowExpanded windowExpanded)
	{
		_windowExpanded = windowExpanded;
	}

	public void UpdateQuota(double usedMb, double totalMb = 300.0)
	{
		base.Dispatcher.Invoke(() =>
		{
			QuotaProgressBar.Maximum = totalMb;
			QuotaProgressBar.Value = Math.Min(usedMb, totalMb);
			QuotaUsedText.Text = $"{usedMb:F1} MB";
		});
	}

	private void DashboardSideMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.Dashboard);
	}

	private void LocationsSideMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.Location);
	}

	private void ResidentialMeshSideMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.ResidentialMesh);
	}

	private void SpeedTestSideMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.SpeedTest);
	}

	private void ProtocolSideMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.Protocol);
	}

	private void SecuritySideMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.Security);
	}

	private void AccountSideMenu_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.Account);
	}

	private void GeneralSideMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.Settings);
	}

	private void NotificationsMenu_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.Notifications);
	}

	private void GetHelpMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		SetMenuOption(SideMenuOption.GetHelp);
	}

	private void UpgradeVipButton_Click(object sender, RoutedEventArgs e)
	{
		SetMenuOption(SideMenuOption.Account);
	}

	// Hover Helpers
	private void ApplyHover(Border b, TextBlock t, System.Windows.Shapes.Path iconPath, SideMenuOption opt)
	{
		if (_selectedSettings != opt)
		{
			b.Background = _hoverBg;
			if (iconPath != null)
			{
				iconPath.Fill = _activeIconFg;
			}
		}
	}

	private void RemoveHover(Border b, TextBlock t, System.Windows.Shapes.Path iconPath, SideMenuOption opt)
	{
		if (_selectedSettings != opt)
		{
			b.Background = _transparentBg;
			if (iconPath != null)
			{
				iconPath.Fill = _normalIconFg;
			}
		}
	}

	private void DashboardSideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(DashboardSideMenu, DashboardTitle, DashboardIcon, SideMenuOption.Dashboard);
	private void DashboardSideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(DashboardSideMenu, DashboardTitle, DashboardIcon, SideMenuOption.Dashboard);

	private void LocationsSideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(LocationsSideMenu, LocationsTitle, LocationsIcon, SideMenuOption.Location);
	private void LocationsSideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(LocationsSideMenu, LocationsTitle, LocationsIcon, SideMenuOption.Location);

	private void ResidentialMeshSideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(ResidentialMeshSideMenu, ResidentialMeshTitle, ResidentialMeshIcon, SideMenuOption.ResidentialMesh);
	private void ResidentialMeshSideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(ResidentialMeshSideMenu, ResidentialMeshTitle, ResidentialMeshIcon, SideMenuOption.ResidentialMesh);

	private void SpeedTestSideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(SpeedTestSideMenu, SpeedTestTitle, SpeedTestIcon, SideMenuOption.SpeedTest);
	private void SpeedTestSideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(SpeedTestSideMenu, SpeedTestTitle, SpeedTestIcon, SideMenuOption.SpeedTest);

	private void ProtocolSideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(ProtocolSideMenu, SettingsProtocolOption, ProtocolIcon, SideMenuOption.Protocol);
	private void ProtocolSideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(ProtocolSideMenu, SettingsProtocolOption, ProtocolIcon, SideMenuOption.Protocol);

	private void SecuritySideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(SecuritySideMenu, SecurityTitle, SecurityIcon, SideMenuOption.Security);
	private void SecuritySideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(SecuritySideMenu, SecurityTitle, SecurityIcon, SideMenuOption.Security);

	private void AccountSideMenu_OnMouseEnter(object sender, MouseEventArgs e) => ApplyHover(AccountSideMenu, AccountTextBlock, AccountIcon, SideMenuOption.Account);
	private void AccountSideMenu_OnMouseLeave(object sender, MouseEventArgs e) => RemoveHover(AccountSideMenu, AccountTextBlock, AccountIcon, SideMenuOption.Account);

	private void GeneralSideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(GeneralSideMenu, SettingsGeneralOption, GeneralIcon, SideMenuOption.Settings);
	private void GeneralSideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(GeneralSideMenu, SettingsGeneralOption, GeneralIcon, SideMenuOption.Settings);

	private void NotificationsSideMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(NotificationsMenu, NotificationsTextBlock, NotificationsIcon, SideMenuOption.Notifications);
	private void NotificationsSideMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(NotificationsMenu, NotificationsTextBlock, NotificationsIcon, SideMenuOption.Notifications);

	public void ShowNewNotificationWarning(bool show = true)
	{
		// Toggle notification badge
	}

	public void OpenProtocolSettings()
	{
		SetMenuOption(SideMenuOption.Protocol);
	}

	private void GetHelpMenu_MouseEnter(object sender, MouseEventArgs e) => ApplyHover(GetHelpMenu, GetHelpTextBlock, GetHelpIcon, SideMenuOption.GetHelp);
	private void GetHelpMenu_MouseLeave(object sender, MouseEventArgs e) => RemoveHover(GetHelpMenu, GetHelpTextBlock, GetHelpIcon, SideMenuOption.GetHelp);
}
