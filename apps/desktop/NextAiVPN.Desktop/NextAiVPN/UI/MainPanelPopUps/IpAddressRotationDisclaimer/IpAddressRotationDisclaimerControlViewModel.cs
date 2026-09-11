using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace NextAiVPN.UI.MainPanelPopUps.IpAddressRotationDisclaimer;

public class IpAddressRotationDisclaimerControlViewModel : ViewModelBase
{
	private Visibility _controlVisibility = Visibility.Collapsed;

	private string _title = "IP Address";

	private string _description = "IP address may rotate, but always stays within your selected location.";

	public Visibility ControlVisibility
	{
		get
		{
			return _controlVisibility;
		}
		set
		{
			if (_controlVisibility != value)
			{
				_controlVisibility = value;
				OnPropertyChanged("ControlVisibility");
			}
		}
	}

	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (_title != value)
			{
				_title = value;
				OnPropertyChanged("Title");
			}
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (_description != value)
			{
				_description = value;
				OnPropertyChanged("Description");
			}
		}
	}

	public ICommand CloseButtonClickCommand { get; set; }

	public IpAddressRotationDisclaimerControlViewModel()
	{
		CloseButtonClickCommand = new ActionCommand(CloseButtonClickCommandExecute);
	}

	private void CloseButtonClickCommandExecute()
	{
		Close();
	}

	private void Close()
	{
		ControlVisibility = Visibility.Collapsed;
	}
}
