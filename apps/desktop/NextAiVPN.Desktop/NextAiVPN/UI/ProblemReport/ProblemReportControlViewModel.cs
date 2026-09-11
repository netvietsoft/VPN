using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NextAiVPN.Common;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.ProblemReport;

public class ProblemReportControlViewModel : ViewModelBase, IDisposable
{
	private readonly VPNWindowExpanded _expandedVpnWindow;

	private readonly IFeedbackService _feedbackService;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private ProblemReportWindow _parentViewModel;

	private DispatcherTimer _timer;

	private bool _disposed;

	private bool _isEmailFocused;

	private bool _isFeedbackFocused;

	private const string EmailPlaceHolder = "Write your email...";

	private const string FeedbackPlaceHolder = "Please describe, with as much details as possible, the problems you’re having.";

	private const string SendFeedbackButtonTitle = "Send";

	private const int EmojiCode = -1;

	private string _emailText = "Write your email...";

	private readonly DoubleAnimation _doubleAnimation = new DoubleAnimation();

	private readonly RotateTransform _rotateTransform = new RotateTransform();

	public Image SendSpinner;

	private string _sendFeedbackButtonContent = "Send";

	private SolidColorBrush _emailCaretBrush;

	private SolidColorBrush _feedbackCaretBrush;

	private SolidColorBrush _emailBorderBrush = GetBrushFromString("#848487");

	private SolidColorBrush _emailForeground = GetBrushFromString("#BFBFBF");

	private string _feedbackText = "Please describe, with as much details as possible, the problems you’re having.";

	private SolidColorBrush _feedbackBrush = GetBrushFromString("#848487");

	private SolidColorBrush _feedbackForeground = GetBrushFromString("#BFBFBF");

	private string _characterCounter = "0";

	private int _controlHeight = 446;

	private int _controlWidth = 360;

	private int _madHeight = 25;

	private int _madWidth = 25;

	private int _mahHeight = 25;

	private int _mahWidth = 25;

	private bool _needToSendDiagnosticFile = true;

	private int _smileHeight = 25;

	private int _smileWidth = 25;

	private Visibility _sendDiagnosticFileVisibility;

	private Visibility _spinnerButtonVisibility = Visibility.Collapsed;

	private Visibility _feedbackThankYouVisibility = Visibility.Collapsed;

	private Visibility _feedbackContentVisibility;

	private Visibility _feedbackErrorVisibility = Visibility.Collapsed;

	private bool _isSendFeedbackButtonEnable = true;

	public string SendFeedbackButtonContent
	{
		get
		{
			return _sendFeedbackButtonContent;
		}
		set
		{
			_sendFeedbackButtonContent = value;
			OnPropertyChanged("SendFeedbackButtonContent");
		}
	}

	public string EmailText
	{
		get
		{
			return _emailText;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				_emailText = "Write your email...";
			}
			_emailText = value;
			OnPropertyChanged("EmailText");
		}
	}

	public SolidColorBrush EmailCaretBrush
	{
		get
		{
			return _emailCaretBrush;
		}
		set
		{
			_emailCaretBrush = value;
			OnPropertyChanged("EmailCaretBrush");
		}
	}

	public SolidColorBrush FeedbackCaretBrush
	{
		get
		{
			return _feedbackCaretBrush;
		}
		set
		{
			_feedbackCaretBrush = value;
			OnPropertyChanged("FeedbackCaretBrush");
		}
	}

	public SolidColorBrush EmailBorderBrush
	{
		get
		{
			return _emailBorderBrush;
		}
		set
		{
			_emailBorderBrush = value;
			OnPropertyChanged("EmailBorderBrush");
		}
	}

	public SolidColorBrush EmailForeground
	{
		get
		{
			return _emailForeground;
		}
		set
		{
			_emailForeground = value;
			OnPropertyChanged("EmailForeground");
		}
	}

	public string FeedbackText
	{
		get
		{
			return _feedbackText;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				_feedbackText = "Please describe, with as much details as possible, the problems you’re having.";
			}
			_feedbackText = value;
			OnPropertyChanged("FeedbackText");
		}
	}

	public SolidColorBrush FeedbackBorderBrush
	{
		get
		{
			return _feedbackBrush;
		}
		set
		{
			_feedbackBrush = value;
			OnPropertyChanged("FeedbackBorderBrush");
		}
	}

	public SolidColorBrush FeedbackForeground
	{
		get
		{
			return _feedbackForeground;
		}
		set
		{
			_feedbackForeground = value;
			OnPropertyChanged("FeedbackForeground");
		}
	}

	public string CharacterCounter
	{
		get
		{
			return _characterCounter;
		}
		set
		{
			_characterCounter = value;
			OnPropertyChanged("CharacterCounter");
		}
	}

	public int ControlHeight
	{
		get
		{
			return _controlHeight;
		}
		set
		{
			_controlHeight = value;
			OnPropertyChanged("ControlHeight");
		}
	}

	public int ControlWidth
	{
		get
		{
			return _controlWidth;
		}
		set
		{
			_controlWidth = value;
			OnPropertyChanged("ControlWidth");
		}
	}

	public int MadHeight
	{
		get
		{
			return _madHeight;
		}
		set
		{
			_madHeight = value;
			OnPropertyChanged("MadHeight");
		}
	}

	public int MadWidth
	{
		get
		{
			return _madWidth;
		}
		set
		{
			_madWidth = value;
			OnPropertyChanged("MadWidth");
		}
	}

	public int MahHeight
	{
		get
		{
			return _mahHeight;
		}
		set
		{
			_mahHeight = value;
			OnPropertyChanged("MahHeight");
		}
	}

	public int MahWidth
	{
		get
		{
			return _mahWidth;
		}
		set
		{
			_mahWidth = value;
			OnPropertyChanged("MahWidth");
		}
	}

	public bool NeedToSendDiagnosticFile
	{
		get
		{
			return _needToSendDiagnosticFile;
		}
		set
		{
			if (_needToSendDiagnosticFile != value)
			{
				_needToSendDiagnosticFile = value;
				OnPropertyChanged("NeedToSendDiagnosticFile");
			}
		}
	}

	public int SmileHeight
	{
		get
		{
			return _smileHeight;
		}
		set
		{
			_smileHeight = value;
			OnPropertyChanged("SmileHeight");
		}
	}

	public int SmileWidth
	{
		get
		{
			return _smileWidth;
		}
		set
		{
			_smileWidth = value;
			OnPropertyChanged("SmileWidth");
		}
	}

	public Visibility SendDiagnosticFileVisibility
	{
		get
		{
			return _sendDiagnosticFileVisibility;
		}
		set
		{
			_sendDiagnosticFileVisibility = value;
			OnPropertyChanged("SendDiagnosticFileVisibility");
		}
	}

	public Visibility SpinnerButtonVisibility
	{
		get
		{
			return _spinnerButtonVisibility;
		}
		set
		{
			_spinnerButtonVisibility = value;
			OnPropertyChanged("SpinnerButtonVisibility");
		}
	}

	public Visibility FeedbackThankYouVisibility
	{
		get
		{
			return _feedbackThankYouVisibility;
		}
		set
		{
			_feedbackThankYouVisibility = value;
			OnPropertyChanged("FeedbackThankYouVisibility");
		}
	}

	public Visibility FeedbackContentVisibility
	{
		get
		{
			return _feedbackContentVisibility;
		}
		set
		{
			_feedbackContentVisibility = value;
			OnPropertyChanged("FeedbackContentVisibility");
		}
	}

	public Visibility FeedbackErrorVisibility
	{
		get
		{
			return _feedbackErrorVisibility;
		}
		set
		{
			_feedbackErrorVisibility = value;
			OnPropertyChanged("FeedbackErrorVisibility");
		}
	}

	public bool IsSendFeedbackButtonEnable
	{
		get
		{
			return _isSendFeedbackButtonEnable;
		}
		set
		{
			_isSendFeedbackButtonEnable = value;
			OnPropertyChanged("_isSendFeedbackButtonEnable");
		}
	}

	public ICommand EmailTextChangedCommand { get; set; }

	public ICommand EmailTextLostFocusCommand { get; set; }

	public ICommand EmailTextGotFocusCommand { get; set; }

	public ICommand FeedbackTextChangedCommand { get; set; }

	public ICommand FeedbackTextLostFocusCommand { get; set; }

	public ICommand FeedbackTextGotFocusCommand { get; set; }

	public ICommand SendFeedbackButtonClickCommand { get; set; }

	public ICommand FeedbackTextLeftMouseDownCommand { get; set; }

	public ICommand EmailTextLeftMouseDownCommand { get; set; }

	public ProblemReportControlViewModel(VPNWindowExpanded expandedVpnWindow, IFeedbackService feedbackService, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		InitializeCommands();
		_expandedVpnWindow = expandedVpnWindow;
		_feedbackService = feedbackService;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		GlobalEvents.StyleChanged += StyleChangedEventStyleChanged;
		SetTimer();
		FeedbackCaretBrush = GetCaretBrush(isTransparent: false);
		EmailCaretBrush = GetCaretBrush(isTransparent: false);
	}

	public void GotFocusFeedbackText()
	{
		FeedbackTextGotFocus();
	}

	private void StyleChangedEventStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		ChangeStyle(appStyle);
	}

	private static SolidColorBrush GetPlaceholderForeground()
	{
		return GetBrushFromString("#BFBFBF");
	}

	private static SolidColorBrush GetCaretBrush(bool isTransparent)
	{
		if (isTransparent)
		{
			return GetBrushFromString("#00FFFFFF");
		}
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Dark)
		{
			return GetBrushFromString("#000000");
		}
		return GetBrushFromString("#FFFFFF");
	}

	private static SolidColorBrush GetBrushFromString(string colorCode)
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(colorCode);
	}

	private static SolidColorBrush GetBorderBrush()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Dark)
		{
			return GetBrushFromString("#059669");
		}
		return GetBrushFromString("#848487");
	}

	private static SolidColorBrush GetForegroundBrush()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Dark)
		{
			return GetBrushFromString("#000000");
		}
		return GetBrushFromString("#FFFFFF");
	}

	private void InitializeCommands()
	{
		EmailTextChangedCommand = new RelayCommand(EmailTextChangedCommandExecute);
		EmailTextLostFocusCommand = new RelayCommand(EmailTextLostFocusCommandExecute);
		EmailTextGotFocusCommand = new RelayCommand(EmailTextGotFocusCommandExecute);
		FeedbackTextChangedCommand = new RelayCommand(FeedbackTextChangedCommandExecute);
		FeedbackTextLostFocusCommand = new RelayCommand(FeedbackTextLostFocusCommandExecute);
		FeedbackTextGotFocusCommand = new RelayCommand(FeedbackTextGotFocusCommandExecute);
		SendFeedbackButtonClickCommand = new RelayCommand(SendFeedbackButtonClickCommandExecute);
		FeedbackTextLeftMouseDownCommand = new RelayCommand(FeedbackTextLeftMouseDownCommandExecute);
		EmailTextLeftMouseDownCommand = new RelayCommand(EmailTextLeftMouseDownCommandExecute);
	}

	private void EmailTextGotFocusCommandExecute(object obj)
	{
		EmailTextGotFocus();
	}

	private void FeedbackTextGotFocusCommandExecute(object obj)
	{
		FeedbackTextGotFocus();
	}

	private void EmailTextLeftMouseDownCommandExecute(object obj)
	{
		EmailTextGotFocus();
	}

	private void FeedbackTextLeftMouseDownCommandExecute(object obj)
	{
		FeedbackTextGotFocus();
	}

	private async void SendFeedbackButtonClickCommandExecute(object obj)
	{
		if (!IsKillSwitchOnDisconnected())
		{
			IsSendFeedbackButtonEnable = false;
			AnimateSpinner(animate: true);
			string message = (IsFeedbackPlaceholder() ? "" : FeedbackText);
			_logger?.Information("Problem report submitted.", "SendFeedbackButtonClickCommandExecute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\ProblemReport\\ProblemReportControlViewModel.cs", 472);
			if (!string.IsNullOrEmpty(EmailText) && !IsEmailPlaceholder())
			{
				_appSettingsHelper.SetValue("Email", EmailText);
			}
			string text = await _feedbackService.SendFeedbackNotification((-1).ToString(), message, NeedToSendDiagnosticFile);
			if (text.Equals("1"))
			{
				ShowThankYouMessage();
				_feedbackService.SaveLastFeedbackClosed(reset: true);
			}
			else if (text.Equals("-1"))
			{
				if ((await _feedbackService.SendFeedbackNotification((-1).ToString(), message, NeedToSendDiagnosticFile)).Equals("1"))
				{
					ShowThankYouMessage();
					FeedbackText = string.Empty;
				}
				else
				{
					ShowErrorMessage();
				}
			}
			else
			{
				ShowErrorMessage();
			}
			IsSendFeedbackButtonEnable = true;
			AnimateSpinner(animate: false);
		}
		else
		{
			ShowErrorMessage();
			IsSendFeedbackButtonEnable = true;
			AnimateSpinner(animate: false);
		}
		_appSettingsHelper.SetValue("Email", string.Empty);
	}

	private void IncreaseFeedbackConnectionCounter()
	{
		int.TryParse(_appSettingsHelper.GetValue("SendFeedbackCounter"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
		result++;
		_appSettingsHelper.SetValue("SendFeedbackCounter", result.ToString());
		_appSettingsHelper.SetValue("LastFeedbackSent", DateTime.Now.ToString("d", CultureInfo.InvariantCulture));
		_appSettingsHelper.SetValue("ConnectionsToShowFeedbackCounter", "0");
	}

	private void ShowThankYouMessage()
	{
		_timer.Start();
		FeedbackThankYouVisibility = Visibility.Visible;
		FeedbackErrorVisibility = Visibility.Collapsed;
		FeedbackContentVisibility = Visibility.Collapsed;
		ControlHeight = 95;
		_parentViewModel.Height = 95.0;
	}

	private void ShowErrorMessage()
	{
		FeedbackThankYouVisibility = Visibility.Collapsed;
		FeedbackErrorVisibility = Visibility.Visible;
		ChangeWindowHeight(60);
	}

	private void SetTimer()
	{
		_timer = new DispatcherTimer();
		_timer.Interval = TimeSpan.FromSeconds(5L);
		_timer.Tick += _timer_Tick;
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		_parentViewModel.Close();
	}

	private void HideErrorMessage()
	{
		FeedbackErrorVisibility = Visibility.Collapsed;
		ChangeWindowHeight(-60);
	}

	private void ChangeWindowHeight(int value)
	{
		if (ControlHeight != 506)
		{
			ControlHeight += value;
			_parentViewModel.Height += value;
		}
	}

	private bool IsKillSwitchOnDisconnected()
	{
		if (!_expandedVpnWindow.SdkObject.NextAiVpnSdkManager.IsConnected)
		{
			return _appSettingsHelper.GetValue("KillSwitch").Equals("1");
		}
		return false;
	}

	private void AnimateSpinner(bool animate)
	{
		_doubleAnimation.From = 0.0;
		_doubleAnimation.To = 360.0;
		_doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		SendSpinner.RenderTransform = _rotateTransform;
		SendSpinner.RenderTransformOrigin = new Point(0.5, 0.5);
		if (animate)
		{
			SendSpinner.Visibility = Visibility.Visible;
			SendFeedbackButtonContent = string.Empty;
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, _doubleAnimation);
		}
		else
		{
			SendFeedbackButtonContent = "Send";
			SendSpinner.Visibility = Visibility.Collapsed;
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
		}
	}

	private void FeedbackTextLostFocusCommandExecute(object obj)
	{
		_isFeedbackFocused = false;
		if (string.IsNullOrEmpty(FeedbackText))
		{
			FeedbackText = "Please describe, with as much details as possible, the problems you’re having.";
			FeedbackForeground = GetBrushFromString("#BFBFBF");
			FeedbackBorderBrush = GetBrushFromString("#848487");
		}
		else
		{
			FeedbackBorderBrush = GetBrushFromString("#848487");
			FeedbackForeground = GetForegroundBrush();
		}
	}

	private void FeedbackTextGotFocus()
	{
		EmailCaretBrush = GetCaretBrush(isTransparent: true);
		_isFeedbackFocused = true;
		if (IsFeedbackPlaceholder())
		{
			FeedbackText = string.Empty;
		}
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			FeedbackBorderBrush = GetBrushFromString("#059669");
			FeedbackForeground = GetBrushFromString("#000000");
		}
		else
		{
			FeedbackForeground = GetBrushFromString("#FFFFFF");
		}
	}

	private void FeedbackTextChangedCommandExecute(object obj)
	{
		if (!(FeedbackText == "Please describe, with as much details as possible, the problems you’re having."))
		{
			CharacterCounter = FeedbackText.Length.ToString();
			FeedbackBorderBrush = GetBorderBrush();
			FeedbackCaretBrush = GetCaretBrush(isTransparent: true);
		}
	}

	private void EmailTextLostFocusCommandExecute(object obj)
	{
		_isEmailFocused = false;
		if (string.IsNullOrEmpty(EmailText))
		{
			EmailText = "Write your email...";
			EmailForeground = GetBrushFromString("#BFBFBF");
			EmailBorderBrush = GetBrushFromString("#848487");
		}
		else
		{
			EmailBorderBrush = GetBrushFromString("#848487");
			EmailForeground = GetForegroundBrush();
		}
	}

	private void EmailTextGotFocus()
	{
		EmailCaretBrush = GetCaretBrush(isTransparent: false);
		_isEmailFocused = true;
		if (IsEmailPlaceholder())
		{
			EmailText = string.Empty;
		}
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			EmailBorderBrush = GetBrushFromString("#059669");
			EmailForeground = GetBrushFromString("#000000");
		}
		else
		{
			EmailForeground = GetBrushFromString("#FFFFFF");
		}
	}

	private void TextBoxesLostFocus()
	{
		_isFeedbackFocused = false;
		_isEmailFocused = false;
		EmailBorderBrush = GetBrushFromString("#848487");
		FeedbackBorderBrush = GetBrushFromString("#848487");
		if (IsEmailPlaceholder())
		{
			EmailForeground = GetPlaceholderForeground();
		}
		if (IsFeedbackPlaceholder())
		{
			FeedbackForeground = GetPlaceholderForeground();
		}
		FeedbackCaretBrush = GetCaretBrush(isTransparent: true);
		EmailCaretBrush = GetCaretBrush(isTransparent: true);
	}

	private bool IsEmailPlaceholder()
	{
		return EmailText.Equals("Write your email...");
	}

	private bool IsFeedbackPlaceholder()
	{
		return FeedbackText.Equals("Please describe, with as much details as possible, the problems you’re having.");
	}

	private void EmailTextChangedCommandExecute(object obj)
	{
		EmailBorderBrush = GetBorderBrush();
		EmailCaretBrush = GetCaretBrush(isTransparent: true);
	}

	public void ChangeStyle(NextAiVPN.Services.Persistence.Style appStyle)
	{
		SolidColorBrush foregroundBrush = GetForegroundBrush();
		SolidColorBrush placeholderForeground = GetPlaceholderForeground();
		FeedbackForeground = (IsFeedbackPlaceholder() ? placeholderForeground : foregroundBrush);
		EmailForeground = (IsEmailPlaceholder() ? placeholderForeground : foregroundBrush);
		SolidColorBrush borderBrush = GetBorderBrush();
		if (_isFeedbackFocused)
		{
			FeedbackBorderBrush = borderBrush;
		}
		if (_isEmailFocused)
		{
			EmailBorderBrush = borderBrush;
		}
	}

	public void GetParentWindow(ProblemReportWindow window)
	{
		_parentViewModel = window;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_disposed = true;
			GlobalEvents.StyleChanged -= StyleChangedEventStyleChanged;
			if (_timer != null)
			{
				_timer.Stop();
				_timer.Tick -= _timer_Tick;
				_timer = null;
			}
		}
	}
}
