using System.Windows;

namespace NextAiVPN.UI.ProblemReport;

public class ProblemReportWindowViewModel : ViewModelBase
{
	private Window _reportWindow;

	private int _windowHeight = 390;

	private int _windowWidth = 360;

	private Visibility _reportControlVisibility;

	public ProblemReportControlViewModel ProblemReportControlViewModel { get; private set; }

	public int WindowHeight
	{
		get
		{
			return _windowHeight;
		}
		set
		{
			_windowHeight = value;
			OnPropertyChanged("WindowHeight");
		}
	}

	public int WindowWidth
	{
		get
		{
			return _windowWidth;
		}
		set
		{
			_windowWidth = value;
			OnPropertyChanged("WindowWidth");
		}
	}

	public Visibility ReportControlVisibility
	{
		get
		{
			return _reportControlVisibility;
		}
		set
		{
			_reportControlVisibility = value;
			OnPropertyChanged("ReportControlVisibility");
		}
	}

	public ProblemReportWindowViewModel(ProblemReportControlViewModel problemReportControlViewModel)
	{
		ProblemReportControlViewModel = problemReportControlViewModel;
	}

	public void GetReportWindow(Window reportWindow)
	{
		_reportWindow = reportWindow;
	}

	public void Close()
	{
		ProblemReportControlViewModel.Dispose();
		_reportWindow.Hide();
	}
}
