using System.Windows;

namespace NextAiVPN.UI.Feedback;

public class FeedbackWindowViewModel : ViewModelBase
{
	private Window _feedbackWindow;

	private int _windowHeight = 535;

	private int _windowWidth = 360;

	private Visibility _feedbackControlVisibility;

	public FeedbackControlViewModel FeedbackControlViewModel { get; private set; }

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

	public Visibility FeedbackControlVisibility
	{
		get
		{
			return _feedbackControlVisibility;
		}
		set
		{
			_feedbackControlVisibility = value;
			OnPropertyChanged("FeedbackControlVisibility");
		}
	}

	public FeedbackWindowViewModel(FeedbackControlViewModel feedbackControlViewModel)
	{
		FeedbackControlViewModel = feedbackControlViewModel;
	}

	public void GetFeedbackWindow(Window feedbackWindow)
	{
		_feedbackWindow = feedbackWindow;
	}

	public void Close()
	{
		if (FeedbackControlViewModel.Closing())
		{
			_feedbackWindow.Hide();
			FeedbackControlViewModel.Dispose();
		}
	}
}
