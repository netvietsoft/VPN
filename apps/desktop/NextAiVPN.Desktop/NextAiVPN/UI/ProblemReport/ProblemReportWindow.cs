using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.ProblemReport;

public partial class ProblemReportWindow : Window, IComponentConnector
{
	public ProblemReportWindow(ProblemReportWindowViewModel problemReportWindowViewModel)
	{
		InitializeComponent();
		base.DataContext = problemReportWindowViewModel;
		ProblemReport.DataContext = problemReportWindowViewModel.ProblemReportControlViewModel;
		problemReportWindowViewModel.GetReportWindow(ReportWindow);
		problemReportWindowViewModel.ProblemReportControlViewModel.GetParentWindow(this);
		problemReportWindowViewModel.ProblemReportControlViewModel.SendSpinner = ProblemReport.SendSpinner;
		ProblemReport.FeedbackThankYou.btnCross.Visibility = Visibility.Collapsed;
	}

	private void ProblemReportWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}
}
