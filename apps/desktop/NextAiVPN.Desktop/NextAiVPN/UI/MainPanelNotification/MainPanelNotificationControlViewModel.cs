using System;
using System.Windows;
using System.Windows.Input;
using NextAiVPN.UI.ProblemReport;

namespace NextAiVPN.UI.MainPanelNotification;

public class MainPanelNotificationControlViewModel : ViewModelBase
{
	private Visibility _userControlVisibility = Visibility.Collapsed;

	private readonly VPNWindowExpanded _expandedVpnWindow;

	public ProblemReportWindow ReportWindow;

	public Visibility UserControlVisibility
	{
		get
		{
			return _userControlVisibility;
		}
		set
		{
			_userControlVisibility = value;
			OnPropertyChanged("UserControlVisibility");
		}
	}

	public ICommand CloseButtonClickCommand { get; set; }

	public ICommand TextBlockLinkClickCommand { get; set; }

	public MainPanelNotificationControlViewModel(VPNWindowExpanded windowExpanded)
	{
		_expandedVpnWindow = windowExpanded;
		CloseButtonClickCommand = new RelayCommand(CloseButtonClickCommandExecute);
		TextBlockLinkClickCommand = new RelayCommand(TextBlockLinkClickCommandExecute);
	}

	private void TextBlockLinkClickCommandExecute(object obj)
	{
		_expandedVpnWindow.FeedbackWindow.ShowIssueTab();
		_expandedVpnWindow.FeedbackWindow.ShowDialog();
		_expandedVpnWindow.FeedbackWindow.ShowFeedbackTab();
	}

	private void ReportWindow_Closed(object sender, EventArgs e)
	{
		ReportWindow = null;
	}

	private void CloseButtonClickCommandExecute(object obj)
	{
		UserControlVisibility = Visibility.Collapsed;
	}
}
