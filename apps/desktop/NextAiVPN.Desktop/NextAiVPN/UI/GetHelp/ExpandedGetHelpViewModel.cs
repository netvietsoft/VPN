using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace NextAiVPN.UI.GetHelp;

public class ExpandedGetHelpViewModel : ViewModelBase, IExpandedGetHelpViewModel
{
	private readonly VPNWindowExpanded _expandedVpnWindow;

	private DispatcherTimer _timer;

	public string MainControlHeader { get; set; } = "Get Help";

	public string SendFeedbackHeader { get; set; } = "Share feedback or report a problem";

	public string SendFeedbackText { get; set; } = "Make NextAiVPN work better for you by sharing feedback or reporting any issues you encounter.";

	public string SendFeedbackText2 { get; set; } = "Share your views with us ";

	public string SendFeedbackLinkText { get; set; } = "Give feedback";

	public string FAQHeader { get; set; } = "Online chat support";

	public string FAQText { get; set; } = "Get help by chatting with our customer support team. ";

	public string FAQText2 { get; set; }

	public string FAQLinkText { get; set; } = "Start live chat";

	public string CustomerSupportHeader { get; set; } = "Frequently asked questions";

	public string CustomerSupportText { get; set; } = "Find answers to common questions about using NextAiVPN.";

	public string CustomerSupportLinkText { get; set; } = "Browse FAQs";

	public ICommand SendFeedbackMouseDownCommand { get; set; }

	public ICommand FAQMouseDownCommand { get; set; }

	public ICommand CustomerSupportMouseDownCommand { get; set; }

	public ExpandedGetHelpViewModel(VPNWindowExpanded expandedVpnWindow)
	{
		_expandedVpnWindow = expandedVpnWindow;
		InitializeCommands();
	}

	private void InitializeCommands()
	{
		SendFeedbackMouseDownCommand = new RelayCommand(SendFeedbackCommandExecute);
		FAQMouseDownCommand = new RelayCommand(FAQCommandExecute);
		CustomerSupportMouseDownCommand = new RelayCommand(CustomerSupportMouseDownCommandExecute);
	}

	private void SendFeedbackCommandExecute(object obj)
	{
		_expandedVpnWindow.ShowFeedbackWindow();
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		if (_expandedVpnWindow.FeedbackWindow != null)
		{
			_expandedVpnWindow.FeedbackWindow.Topmost = false;
			_expandedVpnWindow.FeedbackWindow.Focus();
			_timer.Stop();
			_timer = null;
		}
	}

	private void FAQCommandExecute(object obj)
	{
		_expandedVpnWindow.SdkObject.BrowserLinksOpener.OpenBrowserLink(4);
	}

	private void CustomerSupportMouseDownCommandExecute(object obj)
	{
		_expandedVpnWindow.SdkObject.BrowserLinksOpener.OpenBrowserLink(8);
	}
}
