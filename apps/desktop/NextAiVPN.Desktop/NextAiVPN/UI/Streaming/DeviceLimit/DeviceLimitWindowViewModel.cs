using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Streaming.DeviceLimit;

public class DeviceLimitWindowViewModel : ViewModelBase
{
	private readonly IBrowserLinksOpener _browserLinksOpener;

	private string _message;

	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (!(value == _message))
			{
				_message = value;
				OnPropertyChanged("Message");
			}
		}
	}

	public ICommand XCloseButtonMouseDownCommand { get; set; }

	public ICommand StreamingRulesLinkMouseDownCommand { get; set; }

	public DeviceLimitWindowViewModel(IBrowserLinksOpener browserLinksOpener)
	{
		_browserLinksOpener = browserLinksOpener;
		XCloseButtonMouseDownCommand = new ActionCommand(XCloseButtonMouseDownCommandExecute);
		StreamingRulesLinkMouseDownCommand = new ActionCommand(StreamingRulesLinkMouseDownCommandExecute);
	}

	private void StreamingRulesLinkMouseDownCommandExecute()
	{
		_browserLinksOpener.OpenBrowserLink(6);
	}

	private void XCloseButtonMouseDownCommandExecute(object obj)
	{
		if (obj is Window window)
		{
			window.Close();
		}
	}
}
