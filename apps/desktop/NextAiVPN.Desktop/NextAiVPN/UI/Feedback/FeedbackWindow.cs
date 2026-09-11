using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using NextAiVPN.Common;

namespace NextAiVPN.UI.Feedback;

public partial class FeedbackWindow : Window, IComponentConnector
{
	private readonly FeedbackWindowViewModel _feedbackWindowViewModel;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public FeedbackWindow(IAppSettingsHelper appSettingsHelper, FeedbackWindowViewModel feedbackWindowViewModel)
	{
		InitializeComponent();
		_feedbackWindowViewModel = feedbackWindowViewModel;
		_appSettingsHelper = appSettingsHelper;
		base.DataContext = _feedbackWindowViewModel;
		_feedbackWindowViewModel.GetFeedbackWindow(FeedbackMainWindow);
		FeedbackControl.DataContext = _feedbackWindowViewModel.FeedbackControlViewModel;
		_feedbackWindowViewModel.FeedbackControlViewModel.GetInstances(this, _feedbackWindowViewModel);
		_feedbackWindowViewModel.FeedbackControlViewModel.SendSpinner = FeedbackControl.SendSpinner;
		_feedbackWindowViewModel.FeedbackControlViewModel.SendIssueSpinner = FeedbackControl.SendIssueSpinner;
		WindowHeader.GetParent(this);
	}

	public void ShowIssueTab()
	{
		_feedbackWindowViewModel.FeedbackControlViewModel.SelectedTabIndex = 1;
	}

	public void ShowFeedbackTab()
	{
		_feedbackWindowViewModel.FeedbackControlViewModel.SelectedTabIndex = 0;
	}

	private void FeedbackWindow_OnClosing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		_appSettingsHelper.SetValue("LastFeedbackClosed", DateTime.Now.ToString("d", CultureInfo.InvariantCulture));
		_feedbackWindowViewModel.Close();
	}
}
