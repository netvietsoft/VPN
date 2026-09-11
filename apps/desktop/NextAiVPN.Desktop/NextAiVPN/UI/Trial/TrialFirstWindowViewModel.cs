using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Common;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Trial;

public class TrialFirstWindowViewModel : ViewModelBase
{
	private readonly ISubscriptionInfo _subscriptionInfo;

	private readonly SDKMonitor _sdk;

	private readonly IAccountTypeHelper _accountTypeHelper;

	private readonly INextAiTechnologySubscriptionProlongationHandler _nextaitechnologySubscriptionProlongationHandler;

	private readonly IAppLogger _logger;

	private readonly IBrowserLinksOpener _browserLinksOpener;

	private TrialFirstWindow _trialFirstWindow;

	private DispatcherTimer _timerPurchase;

	private double _firstBlockOpacity = 1.0;

	private double _middleBlockOpacity = 0.6;

	private double _lastBlockOpacity = 1.0;

	private string _mainTitle;

	private string _buttonText;

	private string _thirdLineHeader;

	private string _thirdLineText1;

	private string _thirdLineText2;

	private BitmapImage _shieldImageSource;

	private BitmapImage _bellImageSource;

	private BitmapImage _starImageSource;

	private Visibility _firstLineVisibility;

	private Visibility _secondLineVisibility = Visibility.Collapsed;

	private Visibility _windowVisibility = Visibility.Collapsed;

	private SolidColorBrush _firstLineBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");

	private string _subscriptionPlanText;

	public double FirstBlockOpacity
	{
		get
		{
			return _firstBlockOpacity;
		}
		set
		{
			_firstBlockOpacity = value;
			OnPropertyChanged("FirstBlockOpacity");
		}
	}

	public double MiddleBlockOpacity
	{
		get
		{
			return _middleBlockOpacity;
		}
		set
		{
			_middleBlockOpacity = value;
			OnPropertyChanged("MiddleBlockOpacity");
		}
	}

	public double LastBlockOpacity
	{
		get
		{
			return _lastBlockOpacity;
		}
		set
		{
			_lastBlockOpacity = value;
			OnPropertyChanged("LastBlockOpacity");
		}
	}

	public string MainTitle
	{
		get
		{
			return _mainTitle;
		}
		set
		{
			_mainTitle = value;
			OnPropertyChanged("MainTitle");
		}
	}

	public string ButtonText
	{
		get
		{
			return _buttonText;
		}
		set
		{
			_buttonText = value;
			OnPropertyChanged("ButtonText");
		}
	}

	public string ThirdLineHeader
	{
		get
		{
			return _thirdLineHeader;
		}
		set
		{
			_thirdLineHeader = value;
			OnPropertyChanged("ThirdLineHeader");
		}
	}

	public string ThirdLineText1
	{
		get
		{
			return _thirdLineText1;
		}
		set
		{
			_thirdLineText1 = value;
			OnPropertyChanged("ThirdLineText1");
		}
	}

	public string ThirdLineText2
	{
		get
		{
			return _thirdLineText2;
		}
		set
		{
			_thirdLineText2 = value;
			OnPropertyChanged("ThirdLineText2");
		}
	}

	public BitmapImage ShieldImageSource
	{
		get
		{
			return _shieldImageSource;
		}
		set
		{
			_shieldImageSource = value;
			OnPropertyChanged("ShieldImageSource");
		}
	}

	public BitmapImage BellImageSource
	{
		get
		{
			return _bellImageSource;
		}
		set
		{
			_bellImageSource = value;
			OnPropertyChanged("BellImageSource");
		}
	}

	public BitmapImage StarImageSource
	{
		get
		{
			return _starImageSource;
		}
		set
		{
			_starImageSource = value;
			OnPropertyChanged("StarImageSource");
		}
	}

	public Visibility FirstLineVisibility
	{
		get
		{
			return _firstLineVisibility;
		}
		set
		{
			_firstLineVisibility = value;
			OnPropertyChanged("FirstLineVisibility");
		}
	}

	public Visibility SecondLineVisibility
	{
		get
		{
			return _secondLineVisibility;
		}
		set
		{
			_secondLineVisibility = value;
			OnPropertyChanged("SecondLineVisibility");
		}
	}

	public Visibility WindowVisibility
	{
		get
		{
			return _windowVisibility;
		}
		set
		{
			_windowVisibility = value;
			FillData();
			OnPropertyChanged("WindowVisibility");
		}
	}

	public SolidColorBrush FirstLineBorderBrush
	{
		get
		{
			return _firstLineBorderBrush;
		}
		set
		{
			_firstLineBorderBrush = value;
			OnPropertyChanged("FirstLineBorderBrush");
		}
	}

	public string SubscriptionPlanText
	{
		get
		{
			return _subscriptionPlanText;
		}
		set
		{
			if (_subscriptionPlanText != value)
			{
				_subscriptionPlanText = value;
				OnPropertyChanged("SubscriptionPlanText");
			}
		}
	}

	public ICommand CloseCommand => new ActionCommand((Action)delegate
	{
		_timerPurchase.Stop();
		RequestClose?.Invoke();
	});

	public ICommand StartButtonPressedCommand => new ActionCommand((Action)async delegate
	{
		if (IsTrialLimitReached())
		{
			switch (_accountTypeHelper.GetAccountType())
			{
			case AccountType.NextAiTechnology:
				await _nextaitechnologySubscriptionProlongationHandler.Prolongate();
				break;
			case AccountType.NextAiGlobal:
				_browserLinksOpener.OpenBrowserLink(VPNConstants.Links.SpSSubscriptionLink);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case AccountType.None:
				break;
			}
			if (!_timerPurchase.IsEnabled)
			{
				_timerPurchase.Start();
			}
		}
		else
		{
			CloseCommand.Execute(null);
		}
	});

	public event Action RequestClose;

	private void SetPurchaseTimer()
	{
		_timerPurchase = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(30L)
		};
		_timerPurchase.Tick += SubscriptionCheckTimer_Tick;
	}

	private async void SubscriptionCheckTimer_Tick(object sender, EventArgs e)
	{
		await _subscriptionInfo.RefreshData();
		if (_subscriptionInfo.IsTrialEnded)
		{
			_timerPurchase.Stop();
			_trialFirstWindow.Hide();
		}
	}

	public TrialFirstWindowViewModel(ISubscriptionInfo subscriptionInfo, SDKMonitor sdk, IAccountTypeHelper accountTypeHelper, INextAiTechnologySubscriptionProlongationHandler nextaitechnologySubscriptionProlongationHandler, IAppLogger logger, IBrowserLinksOpener browserLinksOpener)
	{
		_subscriptionInfo = subscriptionInfo;
		_sdk = sdk;
		_accountTypeHelper = accountTypeHelper;
		_nextaitechnologySubscriptionProlongationHandler = nextaitechnologySubscriptionProlongationHandler;
		_logger = logger;
		_browserLinksOpener = browserLinksOpener;
		GlobalEvents.StyleChanged += StyleChanged;
		SetPurchaseTimer();
	}

	public void SetWindow(TrialFirstWindow window)
	{
		_trialFirstWindow = window;
	}

	public void OpenDialog()
	{
		_logger?.Information($"[Trial first] IsTrialEnded = {_subscriptionInfo.IsTrialEnded}", "OpenDialog", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Trial\\TrialFirstWindowViewModel.cs", 383);
		_trialFirstWindow.Owner = _sdk.VpnExpandedWindow;
		FillData();
		_trialFirstWindow?.ShowDialog();
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		FillData();
	}

	private void FillData()
	{
		Application.Current.Dispatcher.InvokeAsync((Func<Task>)async delegate
		{
			SetShieldIcon();
			SetStarIcon();
			SetBellIcon();
			SetFirstLinesVisibility();
			SetTitle();
			SetButtonText();
			SetThirdLineTexts();
			await SetFooter();
			SetOpacity();
		});
	}

	private void SetOpacity()
	{
		if (IsTrialLimitReached())
		{
			FirstBlockOpacity = 0.6;
			LastBlockOpacity = 1.0;
		}
		else
		{
			FirstBlockOpacity = 1.0;
			LastBlockOpacity = 0.6;
		}
	}

	private async Task SetFooter()
	{
		SubscriptionPlanText = await GetPlanStr();
	}

	private async Task<string> GetPlanStr()
	{
		return IsTrialLimitReached() ? "Your plan starts today" : "Your plan starts on Day 31";
	}

	private void SetTitle()
	{
		MainTitle = (IsTrialLimitReached() ? "Trial limit reached" : "NextAiVPN Trial");
	}

	private void SetButtonText()
	{
		ButtonText = (IsTrialLimitReached() ? "Switch to paid" : "Start using NextAiVPN");
	}

	private void SetThirdLineTexts()
	{
		if (IsTrialLimitReached())
		{
			ThirdLineHeader = "Next step";
			ThirdLineText1 = "30GB limit reached.";
			ThirdLineText2 = "Activate plan to continue.";
		}
		else
		{
			ThirdLineHeader = "Trial ends";
			ThirdLineText1 = "Billing begins.";
			ThirdLineText2 = "No hidden fees.";
		}
	}

	private void SetFirstLinesVisibility()
	{
		if (IsTrialLimitReached())
		{
			SecondLineVisibility = Visibility.Visible;
			FirstLineVisibility = Visibility.Collapsed;
		}
		else
		{
			SecondLineVisibility = Visibility.Collapsed;
			FirstLineVisibility = Visibility.Visible;
		}
	}

	private void SetShieldIcon()
	{
		if (IsTrialLimitReached())
		{
			ShieldImageSource = new BitmapImage(new Uri(IconHelper.GetIcon("TrialShieldGray"), UriKind.RelativeOrAbsolute));
		}
		else
		{
			ShieldImageSource = new BitmapImage(new Uri(IconHelper.GetIcon("TrialShieldOrange"), UriKind.RelativeOrAbsolute));
		}
	}

	private void SetStarIcon()
	{
		if (IsTrialLimitReached())
		{
			StarImageSource = new BitmapImage(new Uri(IconHelper.GetIcon("TrialStarOrange"), UriKind.RelativeOrAbsolute));
		}
		else
		{
			StarImageSource = new BitmapImage(new Uri(IconHelper.GetIcon("TrialStarEmpty"), UriKind.RelativeOrAbsolute));
		}
	}

	private void SetBellIcon()
	{
		if (IsTrialLimitReached())
		{
			BellImageSource = new BitmapImage(new Uri(IconHelper.GetIcon("TrialBellGray"), UriKind.RelativeOrAbsolute));
		}
		else
		{
			BellImageSource = new BitmapImage(new Uri(IconHelper.GetIcon("TrialBellEmpty"), UriKind.RelativeOrAbsolute));
		}
	}

	private bool IsTrialLimitReached()
	{
		return _subscriptionInfo.IsTrialLimitReached;
	}
}
