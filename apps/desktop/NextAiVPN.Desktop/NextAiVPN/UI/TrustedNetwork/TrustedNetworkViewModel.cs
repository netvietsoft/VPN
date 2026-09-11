using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.MessageBoxWindows;

namespace NextAiVPN.UI.TrustedNetwork;

public class TrustedNetworkViewModel : ViewModelBase
{
	private readonly VPNWindowExpanded _vpnWindowExpanded;

	public TrustedNetwork Parent;

	public string NetworkName { get; set; }

	public string WiFiLogoImageSource { get; set; }

	public string CloseButtonImageSource { get; set; }

	public ICommand RemoveTrustedNetworkClickCommand { get; set; }

	public TrustedNetworkViewModel(VPNWindowExpanded vpnWindowExpanded)
	{
		CloseButtonImageSource = "/Assets/XButtonGray.png";
		WiFiLogoImageSource = "/Assets/WiFiIcon.png";
		GlobalEvents.StyleChanged += StyleChanged;
		_vpnWindowExpanded = vpnWindowExpanded;
		RemoveTrustedNetworkClickCommand = new RelayCommand(RemoveTrustedNetworkClickCommandExecute);
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style arg2)
	{
		SetDefaultStyle();
		if (NetworkName.Equals(_vpnWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ConnectedNetwork))
		{
			Parent.RemoveTrustedNetwork.Source = new BitmapImage(new Uri("/Assets/XButtonOrange.png", UriKind.Relative));
			Parent.WiFiIcon.Source = new BitmapImage(new Uri("/Assets/WiFiIconOrange.png", UriKind.Relative));
			switch (appStyle)
			{
			case NextAiVPN.Services.Persistence.Style.Dark:
				Parent.MainBorder.Background = new SolidColorBrush();
				Parent.NetworkName.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#10B981");
				Parent.MainBorder.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#10B981");
				Parent.MainBorder.BorderThickness = new Thickness(1.0);
				break;
			case NextAiVPN.Services.Persistence.Style.Light:
				Parent.NetworkName.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
				Parent.MainBorder.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#DCFCE7");
				Parent.MainBorder.BorderThickness = new Thickness(1.0);
				Parent.MainBorder.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#A7F3D0");
				break;
			default:
				throw new ArgumentOutOfRangeException("appStyle", appStyle, null);
			case NextAiVPN.Services.Persistence.Style.Skip:
				break;
			}
		}
	}

	public void SetDefaultStyle()
	{
		Parent.RemoveTrustedNetwork.Source = new BitmapImage(new Uri("/Assets/XButtonGray.png", UriKind.Relative));
		Parent.WiFiIcon.Source = new BitmapImage(new Uri("/Assets/WiFiIcon.png", UriKind.Relative));
		switch (StyleModeDefiner.DefineAppStyle())
		{
		case NextAiVPN.Services.Persistence.Style.Dark:
			Parent.NetworkName.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#94959E");
			Parent.MainBorder.Background = new SolidColorBrush();
			break;
		case NextAiVPN.Services.Persistence.Style.Light:
			Parent.NetworkName.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#94959E");
			Parent.MainBorder.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#F4F4F5");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case NextAiVPN.Services.Persistence.Style.Skip:
			break;
		}
	}

	private void RemoveTrustedNetworkClickCommandExecute(object obj)
	{
		if (!string.IsNullOrEmpty(NetworkName))
		{
			MessageBoxWindow messageBoxWindow = new MessageBoxWindow(new MessageBoxWindowViewModel
			{
				Header = "Delete trusted network?",
				Description = "Would you like to Delete the existing saved network?",
				CancelButtonText = "Cancel",
				OkButtonText = "Delete"
			});
			messageBoxWindow.ShowDialog();
			if (messageBoxWindow.DialogResult)
			{
				_vpnWindowExpanded.TrustedNetworkService.Remove(NetworkName);
				_vpnWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrustedNetworksWindowViewModel.SortTrustedNetworks();
				_vpnWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrustedNetworksWindowViewModel.ShowTrustedNetworkPanels();
			}
		}
	}
}
