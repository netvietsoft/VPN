using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Common;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.Feedback.DiscardChanges;

namespace NextAiVPN.UI.Feedback;

public class FeedbackControlViewModel : ViewModelBase, IDisposable
{
	private readonly VPNWindowExpanded _expandedVpnWindow;

	private readonly IEmailValidator _emailValidator;

	private readonly IFeedbackService _feedbackService;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private FeedbackWindow _parentViewModel;

	private DispatcherTimer _timer;

	private bool _disposed;

	private bool _isIssueValidationError;

	private FeedbackWindowViewModel _feedbackWindowViewModel;

	private bool _isFeedbackErrorShowing;

	private bool _isEmailFocused;

	private bool _isIssueEmailFocused;

	private bool _isFeedbackFocused;

	private bool _isIssueTextFocused;

	private bool _isFeedbackSend;

	private bool _isIssueReportSend;

	private bool _isValidIssueText;

	private bool _isValidIssueEmail;

	private bool _skipIssueEmailValidation;

	private bool _skipIssueTextValidation;

	private const string EmailPlaceHolder = "Enter your email";

	private const string TryAgainButtonTitle = "Try again";

	private const string FeedbackPlaceHolder = "What’s good or could be better?...";

	private const string SendFeedbackButtonTitle = "Send feedback";

	private const string IssueEmailPlaceHolder = "Email for follow-up";

	private const string IssueTextPlaceHolder = "Describe the issue you’re facing...";

	private const string IssueAttachFileTextPlaceHolder = "Attach diagnostic logs for faster resolution";

	private const string SendIssueButtonContentPlaceHolder = "Send issue";

	private const int IssueTabNormalHeight = 500;

	private const int FeedbackTabNormalHeight = 535;

	private const int ThankYouWindowHeight = 120;

	private readonly int _issueTabErrorHeight = 555;

	private readonly int _feedbackTabErrorHeight = 590;

	private const int IssueTextWordsCountValidation = 4;

	private int _emojiCode;

	private readonly DoubleAnimation _doubleAnimation = new DoubleAnimation();

	private readonly RotateTransform _rotateTransform = new RotateTransform();

	public Image SendSpinner;

	public Image SendIssueSpinner;

	private Brush _feedbackTabHeaderForeground;

	private Brush _issueTabHeaderForeground;

	private string _sendFeedbackButtonContent = "Send feedback";

	private string _sendIssueButtonContent = "Send issue";

	private string _emailText = "Enter your email";

	private string _issueEmailText = "Email for follow-up";

	private SolidColorBrush _emailCaretBrush;

	private Brush _issueEmailCaretBrush;

	private SolidColorBrush _feedbackCaretBrush;

	private SolidColorBrush _emailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");

	private SolidColorBrush _issueEmailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");

	private SolidColorBrush _emailForeground = GetPlaceholderForeground();

	private SolidColorBrush _issueEmailForeground = GetPlaceholderForeground();

	private string _feedbackText = "What’s good or could be better?...";

	private string _issueText = "Describe the issue you’re facing...";

	private SolidColorBrush _feedbackBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");

	private Brush _feedbackTabHeaderBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");

	private Brush _issueTabHeaderBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");

	private SolidColorBrush _issueTextBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");

	private SolidColorBrush _feedbackForeground = GetPlaceholderForeground();

	private Brush _issueTextForeground = GetPlaceholderForeground();

	private string _characterCounter = "0";

	private string _issueCharacterCounter = "0";

	private Brush _issueCharacterCounterForeground;

	private Brush _characterCounterForeground;

	private int _controlHeight = 525;

	private int _controlWidth = 360;

	private int _madHeight = 25;

	private int _madWidth = 25;

	private int _mahHeight = 25;

	private int _mahWidth = 25;

	private bool _needToSendDiagnosticFile;

	private bool _needToSendDiagnosticFileIssue;

	private int _smileHeight = 25;

	private int _smileWidth = 25;

	private Visibility _sendDiagnosticFileVisibility = Visibility.Hidden;

	private Visibility _emailErrorMessageVisibility = Visibility.Hidden;

	private Visibility _issueTextErrorMessageVisibility = Visibility.Hidden;

	private Visibility _emailUnderDescriptionVisibility;

	private Visibility _issueTextUnderDescriptionVisibility;

	private Visibility _spinnerButtonVisibility = Visibility.Collapsed;

	private Visibility _feedbackThankYouVisibility = Visibility.Collapsed;

	private Visibility _issueThankYouVisibility = Visibility.Collapsed;

	private Visibility _feedbackIssueCheckValidationImageVisibility = Visibility.Collapsed;

	private Visibility _feedbackCheckValidationImageVisibility = Visibility.Collapsed;

	private Visibility _madEmojiVisibility = Visibility.Collapsed;

	private Visibility _mahEmojiVisibility = Visibility.Collapsed;

	private Visibility _smileEmojiVisibility;

	private Visibility _feedbackContentVisibility;

	private Visibility _feedbackErrorVisibility = Visibility.Collapsed;

	private Visibility _issueValidationErrorMessageVisibility = Visibility.Collapsed;

	private string _issueValidationErrorMessage = string.Empty;

	private bool _isSendFeedbackButtonEnable = true;

	private bool _isSendIssueButtonEnable = true;

	private string _emailErrorMessage;

	private string _issueTextErrorMessage;

	private Thickness _feedbackTabHeaderBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);

	private Thickness _issueEmailBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);

	private Thickness _issueTextBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);

	private Thickness _issueTabHeaderBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);

	private int _selectedTabIndex;

	public string IssueAttachFileText { get; set; } = "Attach diagnostic logs for faster resolution";

	public Brush FeedbackTabHeaderForeground
	{
		get
		{
			return _feedbackTabHeaderForeground;
		}
		set
		{
			_feedbackTabHeaderForeground = value;
			OnPropertyChanged("FeedbackTabHeaderForeground");
		}
	}

	public Brush IssueTabHeaderForeground
	{
		get
		{
			return _issueTabHeaderForeground;
		}
		set
		{
			_issueTabHeaderForeground = value;
			OnPropertyChanged("IssueTabHeaderForeground");
		}
	}

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

	public string SendIssueButtonContent
	{
		get
		{
			return _sendIssueButtonContent;
		}
		set
		{
			_sendIssueButtonContent = value;
			OnPropertyChanged("SendIssueButtonContent");
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
				_emailText = "Enter your email";
			}
			FeedbackCheckValidationImageVisibility = ((!_emailValidator.IsValidEmail(value)) ? Visibility.Collapsed : Visibility.Visible);
			_emailText = value;
			OnPropertyChanged("EmailText");
		}
	}

	public string IssueEmailText
	{
		get
		{
			return _issueEmailText;
		}
		set
		{
			IssueEmailTextValidation(value);
			_issueEmailText = value;
			OnPropertyChanged("IssueEmailText");
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

	public Brush IssueEmailCaretBrush
	{
		get
		{
			return _issueEmailCaretBrush;
		}
		set
		{
			_issueEmailCaretBrush = value;
			OnPropertyChanged("IssueEmailCaretBrush");
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

	public SolidColorBrush IssueEmailBorderBrush
	{
		get
		{
			return _issueEmailBorderBrush;
		}
		set
		{
			_issueEmailBorderBrush = value;
			OnPropertyChanged("IssueEmailBorderBrush");
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

	public SolidColorBrush IssueEmailForeground
	{
		get
		{
			return _issueEmailForeground;
		}
		set
		{
			_issueEmailForeground = value;
			OnPropertyChanged("IssueEmailForeground");
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
				_feedbackText = "What’s good or could be better?...";
			}
			_feedbackText = value;
			OnPropertyChanged("FeedbackText");
		}
	}

	public string IssueText
	{
		get
		{
			return _issueText;
		}
		set
		{
			IssueTextValidation(value);
			_issueText = value;
			OnPropertyChanged("IssueText");
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

	public Brush FeedbackTabHeaderBorderBrush
	{
		get
		{
			return _feedbackTabHeaderBorderBrush;
		}
		set
		{
			_feedbackTabHeaderBorderBrush = value;
			OnPropertyChanged("FeedbackTabHeaderBorderBrush");
		}
	}

	public Brush IssueTabHeaderBorderBrush
	{
		get
		{
			return _issueTabHeaderBorderBrush;
		}
		set
		{
			_issueTabHeaderBorderBrush = value;
			OnPropertyChanged("IssueTabHeaderBorderBrush");
		}
	}

	public SolidColorBrush IssueTextBorderBrush
	{
		get
		{
			return _issueTextBrush;
		}
		set
		{
			_issueTextBrush = value;
			OnPropertyChanged("IssueTextBorderBrush");
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

	public Brush IssueTextForeground
	{
		get
		{
			return _issueTextForeground;
		}
		set
		{
			_issueTextForeground = value;
			OnPropertyChanged("IssueTextForeground");
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

	public string IssueCharacterCounter
	{
		get
		{
			return _issueCharacterCounter;
		}
		set
		{
			_issueCharacterCounter = value;
			SetIssueTextCounterColor(_issueCharacterCounter);
			OnPropertyChanged("IssueCharacterCounter");
		}
	}

	public Brush IssueCharacterCounterForeground
	{
		get
		{
			return _issueCharacterCounterForeground;
		}
		set
		{
			_issueCharacterCounterForeground = value;
			OnPropertyChanged("IssueCharacterCounterForeground");
		}
	}

	public Brush CharacterCounterForeground
	{
		get
		{
			return _characterCounterForeground;
		}
		set
		{
			_characterCounterForeground = value;
			OnPropertyChanged("CharacterCounterForeground");
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
			if (_feedbackWindowViewModel != null)
			{
				_feedbackWindowViewModel.WindowHeight = _controlHeight;
			}
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

	public bool NeedToSendDiagnosticFileIssue
	{
		get
		{
			return _needToSendDiagnosticFileIssue;
		}
		set
		{
			if (_needToSendDiagnosticFileIssue != value)
			{
				_needToSendDiagnosticFileIssue = value;
				OnPropertyChanged("NeedToSendDiagnosticFileIssue");
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

	public Visibility EmailErrorMessageVisibility
	{
		get
		{
			return _emailErrorMessageVisibility;
		}
		set
		{
			_emailErrorMessageVisibility = value;
			OnPropertyChanged("EmailErrorMessageVisibility");
		}
	}

	public Visibility IssueTextErrorMessageVisibility
	{
		get
		{
			return _issueTextErrorMessageVisibility;
		}
		set
		{
			_issueTextErrorMessageVisibility = value;
			OnPropertyChanged("IssueTextErrorMessageVisibility");
		}
	}

	public Visibility EmailUnderDescriptionVisibility
	{
		get
		{
			return _emailUnderDescriptionVisibility;
		}
		set
		{
			_emailUnderDescriptionVisibility = value;
			OnPropertyChanged("EmailUnderDescriptionVisibility");
		}
	}

	public Visibility IssueTextUnderDescriptionVisibility
	{
		get
		{
			return _issueTextUnderDescriptionVisibility;
		}
		set
		{
			_issueTextUnderDescriptionVisibility = value;
			OnPropertyChanged("IssueTextUnderDescriptionVisibility");
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

	public Visibility IssueThankYouVisibility
	{
		get
		{
			return _issueThankYouVisibility;
		}
		set
		{
			_issueThankYouVisibility = value;
			OnPropertyChanged("IssueThankYouVisibility");
		}
	}

	public Visibility FeedbackIssueCheckValidationImageVisibility
	{
		get
		{
			return _feedbackIssueCheckValidationImageVisibility;
		}
		set
		{
			_feedbackIssueCheckValidationImageVisibility = value;
			OnPropertyChanged("FeedbackIssueCheckValidationImageVisibility");
		}
	}

	public Visibility FeedbackCheckValidationImageVisibility
	{
		get
		{
			return _feedbackCheckValidationImageVisibility;
		}
		set
		{
			_feedbackCheckValidationImageVisibility = value;
			OnPropertyChanged("FeedbackCheckValidationImageVisibility");
		}
	}

	public Visibility MadEmojiVisibility
	{
		get
		{
			return _madEmojiVisibility;
		}
		set
		{
			_madEmojiVisibility = value;
			OnPropertyChanged("MadEmojiVisibility");
		}
	}

	public Visibility MahEmojiVisibility
	{
		get
		{
			return _mahEmojiVisibility;
		}
		set
		{
			_mahEmojiVisibility = value;
			OnPropertyChanged("MahEmojiVisibility");
		}
	}

	public Visibility SmileEmojiVisibility
	{
		get
		{
			return _smileEmojiVisibility;
		}
		set
		{
			_smileEmojiVisibility = value;
			OnPropertyChanged("SmileEmojiVisibility");
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

	public Visibility IssueValidationErrorMessageVisibility
	{
		get
		{
			return _issueValidationErrorMessageVisibility;
		}
		set
		{
			_issueValidationErrorMessageVisibility = value;
			ControlHeight = ((_issueValidationErrorMessageVisibility == Visibility.Visible) ? _issueTabErrorHeight : 500);
			OnPropertyChanged("IssueValidationErrorMessageVisibility");
		}
	}

	public string IssueValidationErrorMessage
	{
		get
		{
			return _issueValidationErrorMessage;
		}
		set
		{
			_issueValidationErrorMessage = value;
			OnPropertyChanged("IssueValidationErrorMessage");
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
			OnPropertyChanged("IsSendFeedbackButtonEnable");
		}
	}

	public bool IsSendIssueButtonEnable
	{
		get
		{
			return _isSendIssueButtonEnable;
		}
		set
		{
			_isSendIssueButtonEnable = value;
			OnPropertyChanged("IsSendIssueButtonEnable");
		}
	}

	public string EmailErrorMessage
	{
		get
		{
			return _emailErrorMessage;
		}
		set
		{
			_emailErrorMessage = value;
			OnPropertyChanged("EmailErrorMessage");
		}
	}

	public string IssueTextErrorMessage
	{
		get
		{
			return _issueTextErrorMessage;
		}
		set
		{
			_issueTextErrorMessage = value;
			OnPropertyChanged("IssueTextErrorMessage");
		}
	}

	public Thickness FeedbackTabHeaderBorderThickness
	{
		get
		{
			return _feedbackTabHeaderBorderThickness;
		}
		set
		{
			_feedbackTabHeaderBorderThickness = value;
			OnPropertyChanged("FeedbackTabHeaderBorderThickness");
		}
	}

	public Thickness IssueEmailBorderThickness
	{
		get
		{
			return _issueEmailBorderThickness;
		}
		set
		{
			_issueEmailBorderThickness = value;
			OnPropertyChanged("IssueEmailBorderThickness");
		}
	}

	public Thickness IssueTextBorderThickness
	{
		get
		{
			return _issueTextBorderThickness;
		}
		set
		{
			_issueTextBorderThickness = value;
			OnPropertyChanged("IssueTextBorderThickness");
		}
	}

	public Thickness IssueTabHeaderBorderThickness
	{
		get
		{
			return _issueTabHeaderBorderThickness;
		}
		set
		{
			_issueTabHeaderBorderThickness = value;
			OnPropertyChanged("IssueTabHeaderBorderThickness");
		}
	}

	public int SelectedTabIndex
	{
		get
		{
			return _selectedTabIndex;
		}
		set
		{
			_selectedTabIndex = value;
			if (FeedbackErrorVisibility == Visibility.Visible)
			{
				HideErrorMessage();
			}
			if (IssueValidationErrorMessageVisibility == Visibility.Visible)
			{
				HideIssueErrorMessage();
			}
			if (_selectedTabIndex == 0)
			{
				SelectFeedbackTab();
			}
			if (_selectedTabIndex == 1)
			{
				SelectIssueTab();
			}
			_parentViewModel?.Focus();
			Keyboard.ClearFocus();
			OnPropertyChanged("SelectedTabIndex");
		}
	}

	public ICommand MadMouseDownCommand { get; set; }

	public ICommand MahMouseDownCommand { get; set; }

	public ICommand SmileMouseDownCommand { get; set; }

	public ICommand EmailTextChangedCommand { get; set; }

	public ICommand EmailTextLostFocusCommand { get; set; }

	public ICommand EmailTextGotFocusCommand { get; set; }

	public ICommand EmailTextLeftMouseDownCommand { get; set; }

	public ICommand FeedbackTextChangedCommand { get; set; }

	public ICommand FeedbackTextLostFocusCommand { get; set; }

	public ICommand FeedbackTextGotFocusCommand { get; set; }

	public ICommand FeedbackTextLeftMouseDownCommand { get; set; }

	public ICommand SendFeedbackButtonClickCommand { get; set; }

	public ICommand SendFeedbackTabSelectedCommand { get; set; }

	public ICommand ReportAnIssueTabSelectedCommand { get; set; }

	public ICommand ReportAnIssueButtonClickCommand { get; set; }

	public ICommand IssueTextChangedCommand { get; set; }

	public ICommand IssueTextLostFocusCommand { get; set; }

	public ICommand IssueTextGotFocusCommand { get; set; }

	public ICommand IssueTextLeftMouseDownCommand { get; set; }

	public ICommand IssueEmailTextChangedCommand { get; set; }

	public ICommand IssueEmailTextLostFocusCommand { get; set; }

	public ICommand IssueEmailTextGotFocusCommand { get; set; }

	public ICommand IssueEmailTextLeftMouseDownCommand { get; set; }

	private bool IsFeedbackTab => SelectedTabIndex == 0;

	public FeedbackControlViewModel(VPNWindowExpanded expandedVpnWindow, IEmailValidator emailValidator, IFeedbackService feedbackService, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		InitializeCommands();
		_expandedVpnWindow = expandedVpnWindow;
		_emailValidator = emailValidator;
		_feedbackService = feedbackService;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		GlobalEvents.StyleChanged += StyleChangedEventStyleChanged;
		SetTimer();
		FeedbackCaretBrush = GetCaretBrush(isTransparent: false);
		EmailCaretBrush = GetCaretBrush(isTransparent: false);
		ConfigUi();
	}

	public bool Closing()
	{
		if (_isIssueReportSend)
		{
			_isIssueReportSend = false;
			return true;
		}
		if (IsFeedbackTab)
		{
			return true;
		}
		if ((!string.IsNullOrEmpty(IssueEmailText) && !IsIssueEmailPlaceholder()) || (!string.IsNullOrEmpty(IssueText) && !IsIssuePlaceholder()))
		{
			DiscardChangesWindow discardChangesWindow = new DiscardChangesWindow();
			discardChangesWindow.ShowDialog();
			if (!discardChangesWindow.DialogResult.HasValue)
			{
				return false;
			}
			return discardChangesWindow.DialogResult.Value;
		}
		return true;
	}

	public void ChangeStyle(NextAiVPN.Services.Persistence.Style appStyle)
	{
		SolidColorBrush foregroundBrush = GetForegroundBrush();
		SolidColorBrush placeholderForeground = GetPlaceholderForeground();
		FeedbackForeground = (IsFeedbackPlaceholder() ? placeholderForeground : foregroundBrush);
		EmailForeground = (IsEmailPlaceholder() ? placeholderForeground : foregroundBrush);
		SolidColorBrush borderBrush = GetBorderBrush();
		Brush tabHeaderSelectedBrush = GetTabHeaderSelectedBrush();
		Brush tabHeaderDefaultBrush = GetTabHeaderDefaultBrush();
		if (IsFeedbackTab)
		{
			IssueTabHeaderBorderBrush = tabHeaderDefaultBrush;
			IssueTabHeaderForeground = tabHeaderDefaultBrush;
			FeedbackTabHeaderBorderBrush = tabHeaderSelectedBrush;
			FeedbackTabHeaderForeground = tabHeaderSelectedBrush;
			if (_isFeedbackFocused)
			{
				FeedbackBorderBrush = borderBrush;
			}
			if (_isEmailFocused)
			{
				EmailBorderBrush = borderBrush;
			}
		}
		else
		{
			IssueTabHeaderBorderBrush = tabHeaderSelectedBrush;
			IssueTabHeaderForeground = tabHeaderSelectedBrush;
			FeedbackTabHeaderBorderBrush = tabHeaderDefaultBrush;
			FeedbackTabHeaderForeground = tabHeaderDefaultBrush;
			if (_isIssueTextFocused)
			{
				IssueTextBorderBrush = borderBrush;
			}
			if (_isIssueEmailFocused)
			{
				IssueEmailBorderBrush = borderBrush;
			}
		}
		SolidColorBrush feedbackForeground = (EmailForeground = (SolidColorBrush)(IssueTextForeground = (IssueEmailForeground = GetPlaceholderForeground())));
		FeedbackForeground = feedbackForeground;
		SetIssueCounterDefaultForeground();
		SetCounterDefaultForeground();
	}

	public void GetInstances(FeedbackWindow window, FeedbackWindowViewModel feedbackWindowViewModel)
	{
		_parentViewModel = window;
		_feedbackWindowViewModel = feedbackWindowViewModel;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed || !disposing)
		{
			return;
		}
		_disposed = true;
		try
		{
			if (_isFeedbackSend)
			{
				_appSettingsHelper.SetValue("IsFeedbackDontSendAndClosedManually", "0");
			}
			else
			{
				_appSettingsHelper.SetValue("IsFeedbackDontSendAndClosedManually", "1");
				_appSettingsHelper.SetValue("SendFeedbackCounter", "0");
				_appSettingsHelper.SetValue("ConnectionsAfterSkipCounter", "0");
			}
			SetDefault();
		}
		finally
		{
			GlobalEvents.StyleChanged -= StyleChangedEventStyleChanged;
			if (_timer != null)
			{
				_timer.Stop();
				_timer.Tick -= _timer_Tick;
				_timer = null;
			}
		}
	}

	private void ConfigUi()
	{
		SetIssueBordersThickness();
		IssueCharacterCounter = "0";
		IssueValidationErrorMessage = "Unable to send your feedback. Please check your Internet connection and try again.";
		SelectEmoji(2);
		NeedToSendDiagnosticFile = false;
		NeedToSendDiagnosticFileIssue = true;
		SelectedTabIndex = 0;
		SetIssueCounterDefaultForeground();
		SetCounterDefaultForeground();
		SetDefaultColorFeedbackBorderBrush();
	}

	private void SetIssueBordersThickness()
	{
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark)
		{
			IssueEmailBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);
			IssueTextBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);
		}
		else
		{
			IssueEmailBorderThickness = new Thickness(1.0, 1.0, 1.0, 1.0);
			IssueTextBorderThickness = new Thickness(1.0, 1.0, 1.0, 1.0);
		}
	}

	private void StyleChangedEventStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		ChangeStyle(appStyle);
	}

	private static SolidColorBrush GetPlaceholderForeground()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Light)
		{
			return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(VPNConstants.Colors.ColorB3B3B4);
		}
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(VPNConstants.Colors.Color575758);
	}

	private static SolidColorBrush GetCaretBrush(bool isTransparent)
	{
		if (isTransparent)
		{
			return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00FFFFFF");
		}
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Dark)
		{
			return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
	}

	private static SolidColorBrush GetBorderBrush()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Dark)
		{
			return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
		}
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");
	}

	private static SolidColorBrush GetForegroundBrush()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Dark)
		{
			return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
	}

	private static Brush GetTabHeaderDefaultBrush()
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? "#DCDCDF" : "#94959E");
	}

	private static Brush GetTabHeaderSelectedBrush()
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? VPNConstants.Colors.ColorE66F19 : VPNConstants.Colors.ColorB64D00);
	}

	private void InitializeCommands()
	{
		MadMouseDownCommand = new RelayCommand(MadMouseDownCommandExecute);
		MahMouseDownCommand = new RelayCommand(MahMouseDownCommandExecute);
		SmileMouseDownCommand = new RelayCommand(SmileMouseDownCommandExecute);
		EmailTextChangedCommand = new RelayCommand(EmailTextChangedCommandExecute);
		EmailTextLostFocusCommand = new RelayCommand(EmailTextLostFocusCommandExecute);
		EmailTextGotFocusCommand = new RelayCommand(EmailTextGotFocusCommandExecute);
		EmailTextLeftMouseDownCommand = new RelayCommand(EmailTextLeftMouseDownCommandExecute);
		FeedbackTextChangedCommand = new RelayCommand(FeedbackTextChangedCommandExecute);
		FeedbackTextLostFocusCommand = new RelayCommand(FeedbackTextLostFocusCommandExecute);
		FeedbackTextGotFocusCommand = new RelayCommand(FeedbackTextGotFocusCommandExecute);
		FeedbackTextLeftMouseDownCommand = new RelayCommand(FeedbackTextLeftMouseDownCommandExecute);
		SendFeedbackButtonClickCommand = new RelayCommand(SendFeedbackButtonClickCommandExecute);
		SendFeedbackTabSelectedCommand = new ActionCommand(SendFeedbackTabSelectedCommandExecute);
		ReportAnIssueTabSelectedCommand = new ActionCommand(ReportAnIssueTabSelectedCommandExecute);
		ReportAnIssueButtonClickCommand = new ActionCommand(ReportAnIssueButtonClickCommandExecute);
		IssueTextChangedCommand = new ActionCommand(IssueTextChangedCommandExecute);
		IssueTextLostFocusCommand = new ActionCommand(IssueTextLostFocusCommandExecute);
		IssueTextGotFocusCommand = new ActionCommand(IssueTextGotFocusCommandExecute);
		IssueTextLeftMouseDownCommand = new ActionCommand(IssueTextLeftMouseDownCommandExecute);
		IssueEmailTextChangedCommand = new ActionCommand(IssueEmailTextChangedCommandExecute);
		IssueEmailTextLostFocusCommand = new ActionCommand(IssueEmailTextLostFocusCommandExecute);
		IssueEmailTextGotFocusCommand = new ActionCommand(IssueEmailTextGotFocusCommandExecute);
		IssueEmailTextLeftMouseDownCommand = new ActionCommand(IssueEmailTextLeftMouseDownCommandExecute);
	}

	private void IssueEmailTextLeftMouseDownCommandExecute()
	{
		IssueEmailTextGotFocus();
	}

	private void IssueEmailTextGotFocusCommandExecute()
	{
		IssueEmailTextGotFocus();
	}

	private void IssueEmailTextGotFocus()
	{
		IssueEmailCaretBrush = GetCaretBrush(isTransparent: false);
		_isIssueEmailFocused = true;
		if (IsIssueEmailPlaceholder())
		{
			IssueEmailText = string.Empty;
		}
		else
		{
			IssueEmailText = IssueEmailText;
		}
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			IssueEmailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
			IssueEmailForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		else
		{
			IssueEmailForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private void IssueEmailTextLostFocusCommandExecute()
	{
		_isIssueEmailFocused = false;
		if (string.IsNullOrEmpty(IssueEmailText))
		{
			IssueEmailText = "Email for follow-up";
			IssueEmailForeground = GetPlaceholderForeground();
		}
		else
		{
			IssueEmailForeground = GetForegroundBrush();
		}
		IssueEmailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
		if (!_isValidIssueEmail)
		{
			IssueEmailBorderBrush = GetErrorColorBorder();
		}
	}

	private void IssueEmailTextChangedCommandExecute()
	{
	}

	private void IssueTextLeftMouseDownCommandExecute()
	{
		IssueTextGotFocus();
	}

	private void IssueTextGotFocusCommandExecute()
	{
		IssueTextGotFocus();
	}

	private void IssueTextGotFocus()
	{
		IssueEmailCaretBrush = GetCaretBrush(isTransparent: true);
		_isIssueTextFocused = true;
		if (IsIssuePlaceholder())
		{
			IssueText = string.Empty;
		}
		else
		{
			IssueText = IssueText;
		}
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			IssueTextBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
			IssueTextForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		else
		{
			IssueTextForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private void IssueTextLostFocusCommandExecute()
	{
		_isIssueTextFocused = false;
		if (string.IsNullOrEmpty(IssueText))
		{
			IssueText = "Describe the issue you’re facing...";
			IssueTextForeground = GetPlaceholderForeground();
		}
		else
		{
			IssueTextForeground = GetForegroundBrush();
		}
		IssueTextBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
		if (!_isValidIssueText)
		{
			IssueTextBorderBrush = GetErrorColorBorder();
		}
	}

	private void IssueTextChangedCommandExecute()
	{
		if (!(IssueText == "Describe the issue you’re facing..."))
		{
			IssueCharacterCounter = IssueText.Length.ToString();
			FeedbackCaretBrush = GetCaretBrush(isTransparent: true);
		}
	}

	private async void ReportAnIssueButtonClickCommandExecute()
	{
		if (!IsValidIssueFields())
		{
			return;
		}
		if (!IsKillSwitchOnDisconnected())
		{
			IsSendIssueButtonEnable = false;
			AnimateIssueSpinner(animate: true);
			_logger?.Information("Issue report submitted.", "ReportAnIssueButtonClickCommandExecute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Feedback\\FeedbackControlViewModel.cs", 1602);
			_appSettingsHelper.SetValue("Email", IssueEmailText);
			string text = await _feedbackService.SendFeedbackNotification("-1", IssueText, NeedToSendDiagnosticFileIssue);
			if (text.Equals("1"))
			{
				IssueReportSent();
			}
			else
			{
				if (!text.Equals("-1"))
				{
					AnimateIssueSpinner(animate: false);
					ShowIssueErrorMessage();
					IsSendIssueButtonEnable = true;
					return;
				}
				if (!(await _feedbackService.SendFeedbackNotification("-1", IssueText, NeedToSendDiagnosticFileIssue)).Equals("1"))
				{
					AnimateIssueSpinner(animate: false);
					ShowIssueErrorMessage();
					IsSendIssueButtonEnable = true;
					return;
				}
				IssueReportSent();
			}
			IsSendIssueButtonEnable = true;
			AnimateIssueSpinner(animate: false);
		}
		else
		{
			IsSendIssueButtonEnable = true;
			AnimateIssueSpinner(animate: false);
			ShowIssueErrorMessage();
		}
		_appSettingsHelper.SetValue("Email", string.Empty);
	}

	private bool IsValidIssueFields()
	{
		IssueTextValidation(IssueText);
		IssueEmailTextValidation(IssueEmailText);
		bool flag = true;
		if (string.IsNullOrEmpty(IssueEmailText) || IssueEmailText.Equals("Email for follow-up"))
		{
			ShowSeparatelyEmailValidationError("Please enter your email");
			flag = false;
		}
		if (string.IsNullOrEmpty(IssueText) || IssueText.Equals("Describe the issue you’re facing..."))
		{
			ShowSeparatelyIssueTextValidationError("Tell us about your experience");
			flag = false;
		}
		else if (!IsValidIssueText(IssueText))
		{
			ShowSeparatelyIssueTextValidationError("Write at least 20 characters.");
			flag = false;
		}
		if (flag && _isValidIssueEmail)
		{
			return _isValidIssueText;
		}
		return false;
	}

	private void IssueReportSent()
	{
		HideIssueValidationErrorMessage();
		IncreaseFeedbackConnectionCounter();
		ShowIssueThankYouMessage();
		_isIssueReportSend = true;
	}

	private bool IsValidIssueText(string text = null)
	{
		if (text == null)
		{
			if (!WordsValidation(IssueText))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(IssueText))
			{
				return IssueText.Length >= 20;
			}
			return false;
		}
		return text.Length >= 20;
	}

	private static bool WordsValidation(string text)
	{
		if (text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length >= 4)
		{
			return true;
		}
		return false;
	}

	private void ShowIssueValidationErrorBorder()
	{
		IssueEmailBorderBrush = GetErrorColorBorder();
		IssueTextBorderBrush = GetErrorColorBorder();
	}

	private SolidColorBrush GetErrorColorBorder()
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#F55B5B");
	}

	private void HideIssueValidationErrorBorder()
	{
		IssueEmailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
		IssueTextBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
	}

	private void ShowIssueValidationErrorMessage()
	{
		_isIssueValidationError = true;
		IssueValidationErrorMessageVisibility = Visibility.Visible;
		ControlHeight = 535;
		ShowIssueValidationErrorBorder();
	}

	private void HideIssueValidationErrorMessage()
	{
		_isIssueValidationError = false;
		IssueValidationErrorMessageVisibility = Visibility.Collapsed;
		ControlHeight = 500;
		HideIssueValidationErrorBorder();
	}

	private void SetIssueTextsDefault()
	{
		IssueText = "Describe the issue you’re facing...";
		IssueEmailText = "Email for follow-up";
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			IssueEmailForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
			IssueTextForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		else
		{
			IssueEmailForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
			IssueTextForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private static Brush GetTapHeaderColorBrush(bool isSelected)
	{
		if (!isSelected)
		{
			return GetTabHeaderDefaultBrush();
		}
		return GetTabHeaderSelectedBrush();
	}

	private void FeedbackTextGotFocusCommandExecute(object obj)
	{
		FeedbackTextGotFocus();
	}

	private void ReportAnIssueTabSelectedCommandExecute()
	{
		SelectedTabIndex = 1;
	}

	private void SendFeedbackTabSelectedCommandExecute()
	{
		if (_isIssueValidationError)
		{
			IssueValidationErrorMessageVisibility = Visibility.Collapsed;
		}
		ControlHeight = 535;
		SelectedTabIndex = 0;
	}

	private void SelectIssueTab()
	{
		ControlHeight = 500;
		SendIssueButtonContent = "Send issue";
		IssueTabHeaderBorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
		FeedbackTabHeaderBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);
		FeedbackTabHeaderBorderBrush = GetTapHeaderColorBrush(isSelected: false);
		IssueTabHeaderBorderBrush = GetTapHeaderColorBrush(isSelected: true);
		FeedbackTabHeaderForeground = GetTapHeaderColorBrush(isSelected: false);
		IssueTabHeaderForeground = GetTapHeaderColorBrush(isSelected: true);
		if (string.IsNullOrEmpty(IssueText) || IssueText.Equals("Describe the issue you’re facing..."))
		{
			_skipIssueTextValidation = true;
			SetIssueCounterDefaultForeground();
			HideSeparatelyIssueTextValidationError();
		}
		if (string.IsNullOrEmpty(IssueEmailText) || IssueEmailText.Equals("Email for follow-up"))
		{
			_skipIssueEmailValidation = true;
			HideSeparatelyEmailValidationError();
			FeedbackIssueCheckValidationImageVisibility = Visibility.Collapsed;
		}
	}

	private void SelectFeedbackTab()
	{
		ControlHeight = 535;
		IssueTabHeaderBorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);
		FeedbackTabHeaderBorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
		FeedbackTabHeaderBorderBrush = GetTapHeaderColorBrush(isSelected: true);
		IssueTabHeaderForeground = GetTapHeaderColorBrush(isSelected: false);
		FeedbackTabHeaderForeground = GetTapHeaderColorBrush(isSelected: true);
		IssueTabHeaderForeground = GetTapHeaderColorBrush(isSelected: false);
	}

	private void EmailTextGotFocusCommandExecute(object obj)
	{
		EmailTextGotFocus();
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
			_logger?.Information("Feedback report submitted.", "SendFeedbackButtonClickCommandExecute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Feedback\\FeedbackControlViewModel.cs", 1866);
			if (!string.IsNullOrEmpty(EmailText) && !IsEmailPlaceholder())
			{
				_appSettingsHelper.SetValue("Email", EmailText);
			}
			string text = await _feedbackService.SendFeedbackNotification(_emojiCode.ToString(), message, NeedToSendDiagnosticFile);
			if (text.Equals("1"))
			{
				IncreaseFeedbackConnectionCounter();
				ShowThankYouMessage();
				_feedbackService.SaveLastFeedbackClosed(reset: true);
			}
			else if (text.Equals("-1"))
			{
				if ((await _feedbackService.SendFeedbackNotification(_emojiCode.ToString(), message, NeedToSendDiagnosticFile)).Equals("1"))
				{
					IncreaseFeedbackConnectionCounter();
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
		_isFeedbackSend = true;
	}

	private void HideSeparatelyIssueTextValidationError()
	{
		SetIssueBordersThickness();
		_isValidIssueText = true;
		IssueTextUnderDescriptionVisibility = Visibility.Visible;
		IssueTextErrorMessageVisibility = Visibility.Collapsed;
		_isIssueValidationError = false;
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			IssueTextBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
		}
		else
		{
			IssueTextBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FF848487");
		}
	}

	private void ShowSeparatelyIssueTextValidationError(string message = null)
	{
		IssueTextBorderThickness = new Thickness(1.0, 1.0, 1.0, 1.0);
		_isValidIssueText = false;
		IssueTextUnderDescriptionVisibility = Visibility.Collapsed;
		IssueTextErrorMessageVisibility = Visibility.Visible;
		IssueTextErrorMessage = message;
		_isIssueValidationError = true;
		IssueTextBorderBrush = GetErrorColorBorder();
	}

	private void ShowThankYouMessage()
	{
		StartTimer();
		FeedbackThankYouVisibility = Visibility.Visible;
		IssueThankYouVisibility = Visibility.Collapsed;
		FeedbackContentVisibility = Visibility.Collapsed;
		ControlHeight = 120;
	}

	private void StartTimer()
	{
		if (_timer == null)
		{
			SetTimer();
		}
		_timer?.Start();
	}

	private void ShowIssueThankYouMessage()
	{
		StartTimer();
		IssueThankYouVisibility = Visibility.Visible;
		FeedbackThankYouVisibility = Visibility.Collapsed;
		FeedbackContentVisibility = Visibility.Collapsed;
		ControlHeight = 120;
	}

	private void ShowErrorMessage()
	{
		_isFeedbackErrorShowing = true;
		FeedbackThankYouVisibility = Visibility.Collapsed;
		FeedbackErrorVisibility = Visibility.Visible;
		ControlHeight = _feedbackTabErrorHeight;
		SendFeedbackButtonContent = "Try again";
	}

	private void HideErrorMessage()
	{
		_isFeedbackErrorShowing = false;
		FeedbackThankYouVisibility = Visibility.Collapsed;
		FeedbackErrorVisibility = Visibility.Collapsed;
		SendFeedbackButtonContent = "Send feedback";
		ControlHeight = 535;
	}

	private void ShowIssueErrorMessage()
	{
		IssueValidationErrorMessageVisibility = Visibility.Visible;
		IssueThankYouVisibility = Visibility.Collapsed;
		ControlHeight = _issueTabErrorHeight;
		SendIssueButtonContent = "Try again";
	}

	private void HideIssueErrorMessage()
	{
		IssueValidationErrorMessageVisibility = Visibility.Collapsed;
		IssueThankYouVisibility = Visibility.Collapsed;
	}

	private void SetTimer()
	{
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(3L)
		};
		_timer.Tick += _timer_Tick;
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		_parentViewModel.Close();
		if (_timer != null)
		{
			SetDefault();
			_timer.Stop();
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

	private void AnimateIssueSpinner(bool animate)
	{
		_doubleAnimation.From = 0.0;
		_doubleAnimation.To = 360.0;
		_doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		SendIssueSpinner.RenderTransform = _rotateTransform;
		SendIssueSpinner.RenderTransformOrigin = new Point(0.5, 0.5);
		if (animate)
		{
			SendIssueSpinner.Visibility = Visibility.Visible;
			SendIssueButtonContent = string.Empty;
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, _doubleAnimation);
		}
		else
		{
			SendIssueButtonContent = "Send issue";
			SendIssueSpinner.Visibility = Visibility.Collapsed;
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
		}
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
			SendFeedbackButtonContent = (_isFeedbackErrorShowing ? "Try again" : "Send feedback");
			SendSpinner.Visibility = Visibility.Collapsed;
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
		}
	}

	private void FeedbackTextLostFocusCommandExecute(object obj)
	{
		_isFeedbackFocused = false;
		if (string.IsNullOrEmpty(FeedbackText))
		{
			FeedbackText = "What’s good or could be better?...";
			FeedbackForeground = GetPlaceholderForeground();
		}
		else
		{
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
			FeedbackForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		else
		{
			FeedbackForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private void FeedbackTextChangedCommandExecute(object obj)
	{
		if (!(FeedbackText == "What’s good or could be better?..."))
		{
			CharacterCounter = FeedbackText.Length.ToString();
			FeedbackCaretBrush = GetCaretBrush(isTransparent: true);
		}
	}

	private void EmailTextLostFocusCommandExecute(object obj)
	{
		_isEmailFocused = false;
		if (string.IsNullOrEmpty(EmailText))
		{
			EmailText = "Enter your email";
			EmailForeground = GetPlaceholderForeground();
		}
		else
		{
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
			EmailForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		else
		{
			EmailForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private void TextBoxesLostFocus()
	{
		_isFeedbackFocused = false;
		_isEmailFocused = false;
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

	private void SetDefaultColorFeedbackBorderBrush()
	{
		EmailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
		FeedbackBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
	}

	private bool IsEmailPlaceholder()
	{
		return EmailText.Equals("Enter your email");
	}

	private bool IsIssueEmailPlaceholder()
	{
		return IssueEmailText.Equals("Email for follow-up");
	}

	private bool IsFeedbackPlaceholder()
	{
		return FeedbackText.Equals("What’s good or could be better?...");
	}

	private bool IsIssuePlaceholder()
	{
		return IssueText.Equals("Describe the issue you’re facing...");
	}

	private void EmailTextChangedCommandExecute(object obj)
	{
		EmailCaretBrush = GetCaretBrush(isTransparent: true);
	}

	private void SmileMouseDownCommandExecute(object obj)
	{
		SelectEmoji(3);
	}

	private void MahMouseDownCommandExecute(object obj)
	{
		SelectEmoji(2);
	}

	private void MadMouseDownCommandExecute(object obj)
	{
		SelectEmoji(1);
	}

	private void SelectEmoji(int emojiCode)
	{
		_emojiCode = emojiCode;
		switch (emojiCode)
		{
		case 0:
		{
			int madHeight = (SmileWidth = 25);
			SmileHeight = madHeight;
			madHeight = (MahWidth = 25);
			MahHeight = madHeight;
			madHeight = (MadWidth = 25);
			MadHeight = madHeight;
			break;
		}
		case 1:
		{
			int madHeight = (SmileWidth = 25);
			SmileHeight = madHeight;
			madHeight = (MahWidth = 25);
			MahHeight = madHeight;
			madHeight = (MadWidth = 32);
			MadHeight = madHeight;
			break;
		}
		case 2:
		{
			int madHeight = (MadWidth = 25);
			MadHeight = madHeight;
			madHeight = (SmileWidth = 25);
			SmileHeight = madHeight;
			madHeight = (MahWidth = 32);
			MahHeight = madHeight;
			break;
		}
		case 3:
		{
			int madHeight = (MadWidth = 25);
			MadHeight = madHeight;
			madHeight = (MahWidth = 25);
			MahHeight = madHeight;
			madHeight = (SmileWidth = 32);
			SmileHeight = madHeight;
			break;
		}
		default:
			throw new InvalidOperationException("Wrong emoji code");
		}
		SetEmojiBorderVisibility(emojiCode);
		TextBoxesLostFocus();
	}

	private void SetEmojiBorderVisibility(int emojiCode)
	{
		switch (emojiCode)
		{
		case 0:
			MadEmojiVisibility = Visibility.Collapsed;
			MahEmojiVisibility = Visibility.Collapsed;
			SmileEmojiVisibility = Visibility.Collapsed;
			break;
		case 1:
			MadEmojiVisibility = Visibility.Visible;
			MahEmojiVisibility = Visibility.Collapsed;
			SmileEmojiVisibility = Visibility.Collapsed;
			break;
		case 2:
			MadEmojiVisibility = Visibility.Collapsed;
			MahEmojiVisibility = Visibility.Visible;
			SmileEmojiVisibility = Visibility.Collapsed;
			break;
		case 3:
			MadEmojiVisibility = Visibility.Collapsed;
			MahEmojiVisibility = Visibility.Collapsed;
			SmileEmojiVisibility = Visibility.Visible;
			break;
		default:
			throw new InvalidOperationException("Wrong emoji code");
		}
	}

	private void SetDefault()
	{
		HideSeparatelyEmailValidationError();
		HideSeparatelyIssueTextValidationError();
		FeedbackIssueCheckValidationImageVisibility = Visibility.Collapsed;
		SelectedTabIndex = 0;
		SetIssueCounterDefaultForeground();
		SetIssueTextsDefault();
		SetFeedbackTextsDefault();
		FeedbackContentVisibility = Visibility.Visible;
		FeedbackThankYouVisibility = Visibility.Collapsed;
		FeedbackErrorVisibility = Visibility.Collapsed;
		IssueThankYouVisibility = Visibility.Collapsed;
		IssueValidationErrorMessageVisibility = Visibility.Collapsed;
		SelectEmoji(2);
		ControlHeight = (IsFeedbackTab ? 535 : 500);
		SendIssueButtonContent = "Send issue";
		SendFeedbackButtonContent = "Send feedback";
		IssueTextForeground = GetPlaceholderForeground();
		IssueEmailForeground = GetPlaceholderForeground();
		CharacterCounter = "0";
		IssueCharacterCounter = "0";
	}

	private void SetFeedbackTextsDefault()
	{
		FeedbackText = "What’s good or could be better?...";
		EmailText = "Enter your email";
	}

	private void IssueEmailTextValidation(string value)
	{
		if (_skipIssueEmailValidation)
		{
			_skipIssueEmailValidation = false;
		}
		else if (SelectedTabIndex != 0)
		{
			if (string.IsNullOrEmpty(value) || value.Equals("Email for follow-up"))
			{
				_issueEmailText = "Email for follow-up";
				ShowSeparatelyEmailValidationError("Please enter your email");
			}
			else if (!_emailValidator.IsValidEmail(value))
			{
				ShowSeparatelyEmailValidationError("Please enter a valid email address");
			}
			else
			{
				HideSeparatelyEmailValidationError();
			}
		}
	}

	private void HideSeparatelyEmailValidationError()
	{
		SetIssueBordersThickness();
		_isValidIssueEmail = true;
		EmailUnderDescriptionVisibility = Visibility.Visible;
		EmailErrorMessageVisibility = Visibility.Collapsed;
		FeedbackIssueCheckValidationImageVisibility = Visibility.Visible;
		_isIssueValidationError = false;
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			IssueEmailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
		}
		else
		{
			IssueEmailBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FF848487");
		}
	}

	private void ShowSeparatelyEmailValidationError(string message = null)
	{
		IssueEmailBorderThickness = new Thickness(1.0, 1.0, 1.0, 1.0);
		_isValidIssueEmail = false;
		EmailUnderDescriptionVisibility = Visibility.Collapsed;
		EmailErrorMessageVisibility = Visibility.Visible;
		FeedbackIssueCheckValidationImageVisibility = Visibility.Collapsed;
		EmailErrorMessage = message;
		if (IssueValidationErrorMessageVisibility != Visibility.Visible)
		{
			_isIssueValidationError = true;
			IssueEmailBorderBrush = GetErrorColorBorder();
		}
	}

	private void IssueTextValidation(string value)
	{
		if (_skipIssueTextValidation)
		{
			_skipIssueTextValidation = false;
		}
		else if (SelectedTabIndex != 0)
		{
			if (string.IsNullOrEmpty(value) || value.Equals("Describe the issue you’re facing..."))
			{
				_issueText = "Describe the issue you’re facing...";
				ShowSeparatelyIssueTextValidationError("Tell us about your experience");
			}
			else if (!IsValidIssueText(value))
			{
				ShowSeparatelyIssueTextValidationError("Write at least 20 characters.");
			}
			else
			{
				HideSeparatelyIssueTextValidationError();
			}
		}
	}

	private void SetIssueTextCounterColor(string value)
	{
		if (int.Parse(value, CultureInfo.InvariantCulture) < 20)
		{
			IssueCharacterCounterForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? VPNConstants.Colors.ColorFF553B : "#D91300");
		}
		else
		{
			SetIssueCounterDefaultForeground();
		}
	}

	private void SetIssueCounterDefaultForeground()
	{
		IssueCharacterCounterForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? VPNConstants.Colors.ColorB3B3B4 : VPNConstants.Colors.Color575758);
	}

	private void SetCounterDefaultForeground()
	{
		CharacterCounterForeground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? VPNConstants.Colors.ColorB3B3B4 : VPNConstants.Colors.Color575758);
	}
}
