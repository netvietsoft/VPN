using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NextAiVPN.Common;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.MessageBoxWindows.AutoRenewal;

namespace NextAiVPN.UI.Account;

public class ExpandedAccountViewModel : ViewModelBase
{
	private const string TurnOnAutoRenew = "Turn on auto-renew";

	private readonly AutoRenewalMessageBoxWindow _autoRenewalMessageBoxWindow;

	private readonly ISubscriptionInfo _subscriptionInfo;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAnalyticsService _analyticsService;

	private readonly IAppLogger _logger;

	private readonly IBrowserLinksOpener _browserLinksOpener;

	private readonly INextAiTechnologySubscriptionProlongationHandler _nextaitechnologySubscriptionProlongationHandler;

	private readonly VPNWindowExpanded _expandedWindow;

	private DispatcherTimer _subscriptionFetchTimer;

	private string _usernameText = "Pro Member (nextai@enterprise.local)";

	private string _buttonText = "Manage Subscription";

	private string _expirationDateText = "Active Lifetime VIP License";

	private string _autoRenewalText = "Enabled (Annual Auto-Renew)";

	private string _subscriptionHeader = "Subscription";

	private string _currentPlanText = "Residential Gateway Mesh VIP (Unlimited Quota)";

	private string _renewSubscriptionText = "Manage Plan";

	private string _autoRenewalMessageText = "Hardware-bound device activation active.";

	private string _expiredMessageText;

	private Visibility _expiredMessageVisibility = Visibility.Collapsed;

	private Visibility _currentPlanStackPanelVisibility = Visibility.Visible;

	private Visibility _renewSubscriptionTextVisibility = Visibility.Collapsed;

	private Visibility _dataStackPanelVisibility = Visibility.Visible;

	public string MainControlHeader { get; set; } = "Account";

	public string UsernameHeader { get; set; } = "Username";

	public string PlanHeader { get; set; } = "Plan";

	public string AutoRenewalHeader { get; set; } = "Auto-renewal";

	private string _machineHwidText = "HWID-PC-WIN11-8F92-A3B1-94E2";

	public string MachineHwidText
	{
		get
		{
			if (string.IsNullOrEmpty(_machineHwidText) || _machineHwidText.Contains("8F92"))
			{
				try
				{
					string mName = Environment.MachineName;
					string uName = Environment.UserName;
					using var sha = System.Security.Cryptography.SHA256.Create();
					byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(mName + ":" + uName + ":NextAiHWID"));
					string hex = BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
					_machineHwidText = $"HWID-PC-{mName.ToUpper()}-{hex}";
				}
				catch
				{
					_machineHwidText = "HWID-PC-WIN11-8F92-A3B1-94E2";
				}
			}
			return _machineHwidText;
		}
		set
		{
			_machineHwidText = value;
			OnPropertyChanged("MachineHwidText");
		}
	}

	public string UsernameText
	{
		get
		{
			return _usernameText;
		}
		set
		{
			_usernameText = value;
			OnPropertyChanged("UsernameText");
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
			if (!(value == _buttonText))
			{
				_buttonText = value;
				OnPropertyChanged("ButtonText");
			}
		}
	}

	public string ExpirationDateText
	{
		get
		{
			return _expirationDateText;
		}
		set
		{
			if (!(value == _expirationDateText))
			{
				_expirationDateText = value;
				OnPropertyChanged("ExpirationDateText");
			}
		}
	}

	public string AutoRenewalText
	{
		get
		{
			return _autoRenewalText;
		}
		set
		{
			if (!(value == _autoRenewalText))
			{
				_autoRenewalText = value;
				OnPropertyChanged("AutoRenewalText");
			}
		}
	}

	public string SubscriptionHeader
	{
		get
		{
			return _subscriptionHeader;
		}
		set
		{
			if (!(value == _subscriptionHeader))
			{
				_subscriptionHeader = value;
				OnPropertyChanged("SubscriptionHeader");
			}
		}
	}

	public string CurrentPlanText
	{
		get
		{
			return _currentPlanText;
		}
		set
		{
			_currentPlanText = value;
			OnPropertyChanged("CurrentPlanText");
		}
	}

	public string RenewSubscriptionText
	{
		get
		{
			return _renewSubscriptionText;
		}
		set
		{
			if (!(value == _renewSubscriptionText))
			{
				_renewSubscriptionText = value;
				OnPropertyChanged("RenewSubscriptionText");
			}
		}
	}

	public string AutoRenewalMessageText
	{
		get
		{
			return _autoRenewalMessageText;
		}
		set
		{
			_autoRenewalMessageText = value;
			OnPropertyChanged("AutoRenewalMessageText");
		}
	}

	public string ExpiredMessageText
	{
		get
		{
			return _expiredMessageText;
		}
		set
		{
			_expiredMessageText = value;
			OnPropertyChanged("ExpiredMessageText");
		}
	}

	public Visibility ExpiredMessageVisibility
	{
		get
		{
			return _expiredMessageVisibility;
		}
		set
		{
			_expiredMessageVisibility = value;
			OnPropertyChanged("ExpiredMessageVisibility");
		}
	}

	public Visibility CurrentPlanStackPanelVisibility
	{
		get
		{
			return _currentPlanStackPanelVisibility;
		}
		set
		{
			_currentPlanStackPanelVisibility = value;
			OnPropertyChanged("CurrentPlanStackPanelVisibility");
		}
	}

	public Visibility RenewSubscriptionTextVisibility
	{
		get
		{
			return _renewSubscriptionTextVisibility;
		}
		set
		{
			_renewSubscriptionTextVisibility = value;
			OnPropertyChanged("RenewSubscriptionTextVisibility");
		}
	}

	public Visibility DataStackPanelVisibility
	{
		get
		{
			return _dataStackPanelVisibility;
		}
		set
		{
			_dataStackPanelVisibility = value;
			OnPropertyChanged("DataStackPanelVisibility");
		}
	}

	public ICommand SignOutMouseDownCommand { get; set; }

	public ICommand ExtendSubscriptionButtonClickCommand { get; set; }

	public ExpandedAccountViewModel(VPNWindowExpanded expandedWindow, AutoRenewalMessageBoxWindow autoRenewalMessageBoxWindow, ISubscriptionInfo subscriptionInfo, INextAiTechnologySubscriptionProlongationHandler nextaitechnologySubscriptionProlongationHandler, IBugsnagService bugsnagService, IAnalyticsService analyticsService, IAppLogger logger, IBrowserLinksOpener browserLinksOpener)
	{
		_bugsnagService = bugsnagService;
		_analyticsService = analyticsService;
		_logger = logger;
		_browserLinksOpener = browserLinksOpener;
		_autoRenewalMessageBoxWindow = autoRenewalMessageBoxWindow;
		_subscriptionInfo = subscriptionInfo;
		_nextaitechnologySubscriptionProlongationHandler = nextaitechnologySubscriptionProlongationHandler;
		_expandedWindow = expandedWindow;
		SignOutMouseDownCommand = new RelayCommand(SignOutMouseDownCommandExecute);
		ExtendSubscriptionButtonClickCommand = new RelayCommand(ExtendSubscriptionButtonClickCommandExecute);
		SetTimer();
	}

	public void RefreshData()
	{
		Task.Run(async delegate
		{
			try
			{
				if (_subscriptionInfo != null)
				{
					await _subscriptionInfo.RefreshData();
				}
			}
			catch { }
			DisplayAccountData();
		});
	}

	public void OnTabChanged(SideMenuOption tabIndex)
	{
		if (tabIndex == SideMenuOption.Account)
		{
			DisplayAccountData();
		}
		else if (_subscriptionFetchTimer != null && _subscriptionFetchTimer.IsEnabled)
		{
			_subscriptionFetchTimer.Stop();
		}
	}

	private void SetTimer()
	{
		_subscriptionFetchTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(30L)
		};
		_subscriptionFetchTimer.Tick += SubscriptionFetchTimerOnTick;
	}

	private void SubscriptionFetchTimerOnTick(object sender, EventArgs e)
	{
		Task.Run(async delegate
		{
			await _subscriptionInfo.RefreshData();
			if (RenewSubscriptionText.Equals("Switch to paid") && !_subscriptionInfo.IsTrial)
			{
				_subscriptionFetchTimer.Stop();
				DisplayAccountData();
			}
			else if (_subscriptionInfo.Subscription != null && _subscriptionInfo.Subscription.Autorenewal && _subscriptionFetchTimer.IsEnabled)
			{
				_subscriptionFetchTimer.Stop();
				DisplayAccountData();
			}
		});
	}

	private void CheckSubscription()
	{
		if (IsSubscriptionExpire())
		{
			CurrentPlanStackPanelVisibility = Visibility.Collapsed;
			DateTime? expiresAt = _subscriptionInfo?.Subscription?.ExpiresAt;
			if (expiresAt.HasValue)
			{
				DateTime valueOrDefault = expiresAt.GetValueOrDefault();
				if (IsDayPassed(valueOrDefault))
				{
					ShowExpiredMessage($"Subscription expired ({valueOrDefault:d})");
				}
				else
				{
					int num = Math.Abs((DateTime.Now.Date - valueOrDefault.Date).Days);
					ShowExpiredMessage(num switch
					{
						0 => "Subscription expires today", 
						1 => "Subscription expires tomorrow", 
						_ => $"Subscription expires in {num} days ({valueOrDefault:d})", 
					});
				}
			}
		}
		else
		{
			CurrentPlanStackPanelVisibility = Visibility.Visible;
			ExpiredMessageVisibility = Visibility.Collapsed;
		}
	}

	private void CheckIfTrialEndsSoon()
	{
		try
		{
			if (IsTrialExpire())
			{
				CurrentPlanStackPanelVisibility = Visibility.Collapsed;
				ExpiredMessageVisibility = Visibility.Visible;
				DateTime? trialEnd = _subscriptionInfo?.Subscription?.TrialEnd;
				if (trialEnd.HasValue)
				{
					DateTime valueOrDefault = trialEnd.GetValueOrDefault();
					if (IsDayPassed(valueOrDefault))
					{
						ShowExpiredMessage($"Subscription expired ({valueOrDefault:d})");
					}
					else
					{
						int num = Math.Abs((DateTime.Now.Date - valueOrDefault).Days);
						ShowExpiredMessage(num switch
						{
							0 => "Trial expires today", 
							1 => "Trial expires tomorrow", 
							_ => $"Trial expires in {num} days ({valueOrDefault:d})", 
						});
					}
				}
			}
			else
			{
				CurrentPlanStackPanelVisibility = Visibility.Visible;
				ExpiredMessageVisibility = Visibility.Collapsed;
			}
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "CheckIfTrialEndsSoon", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Account\\ExpandedAccountViewModel.cs", 446);
		}
	}

	private bool IsSubscriptionExpire()
	{
		if (_subscriptionInfo?.IsSubscriptionExpireSoon == true)
		{
			return !(_subscriptionInfo?.Subscription?.Autorenewal ?? true);
		}
		return false;
	}

	private bool IsTrialExpire()
	{
		if (_subscriptionInfo?.IsTrialExpireSoon == true)
		{
			return !(_subscriptionInfo?.Subscription?.Autorenewal ?? true);
		}
		return false;
	}

	private void DisplayAccountData()
	{
		UsernameText = !string.IsNullOrEmpty(_subscriptionInfo?.UserName) ? _subscriptionInfo.UserName : "Pro Member (nextai@enterprise.local)";
		CurrentPlanText = "Residential Gateway Mesh VIP (Unlimited Quota)";
		ExpirationDateText = "Active Lifetime VIP License";
		AutoRenewalText = "Enabled (Annual Auto-Renew)";
		CurrentPlanStackPanelVisibility = Visibility.Visible;
		DataStackPanelVisibility = Visibility.Visible;
	}

	private void SetRenewSubscriptionTextVisibility(ISubscriptionInfo subscriptionInfo)
	{
		if (subscriptionInfo == null)
		{
			RenewSubscriptionTextVisibility = Visibility.Collapsed;
			return;
		}
		if (subscriptionInfo.IsTrial)
		{
			if (subscriptionInfo.IsTrialExpireSoon || subscriptionInfo.Subscription?.Autorenewal == true)
			{
				RenewSubscriptionTextVisibility = Visibility.Collapsed;
			}
			else
			{
				RenewSubscriptionTextVisibility = Visibility.Visible;
			}
		}
		else if (subscriptionInfo.Subscription?.Autorenewal == true)
		{
			RenewSubscriptionTextVisibility = Visibility.Collapsed;
		}
		else
		{
			RenewSubscriptionTextVisibility = (IsSubscriptionExpire() ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private void SetRenewSubscriptionText()
	{
		RenewSubscriptionText = (_subscriptionInfo?.IsTrial == true ? "Switch to paid" : "Renew Subscription");
	}

	private void SetRenewSubscriptionSectionView(ISubscriptionInfo subscriptionInfo)
	{
		if (subscriptionInfo?.Subscription == null)
		{
			AutoRenewalMessageText = "Renews automatically when your plan ends.";
			RenewSubscriptionText = "Turn on auto-renew";
			return;
		}
		if (subscriptionInfo.IsTrial)
		{
			if (subscriptionInfo.Subscription.Autorenewal)
			{
				AutoRenewalMessageText = "You won’t be charged until your trial ends.";
				RenewSubscriptionText = "Switch to paid";
			}
			else
			{
				AutoRenewalMessageText = "Renews automatically when your plan ends.";
				RenewSubscriptionText = "Turn on auto-renew";
			}
		}
		else if (!subscriptionInfo.Subscription.Autorenewal)
		{
			AutoRenewalMessageText = "Renews automatically when your plan ends.";
			RenewSubscriptionText = "Turn on auto-renew";
		}
	}

	private void SetExpiredMessageSectionVisibility(ISubscriptionInfo subscriptionInfo)
	{
		if (subscriptionInfo == null)
		{
			ExpiredMessageVisibility = Visibility.Collapsed;
			return;
		}
		if (subscriptionInfo.IsTrial)
		{
			CheckIfTrialEndsSoon();
		}
		else
		{
			CheckSubscription();
		}
	}

	private void SetExpirationDateText(ISubscriptionInfo subscriptionInfo)
	{
		if (subscriptionInfo?.Subscription == null)
		{
			ExpirationDateText = "Active (VIP)";
			return;
		}
		DateTime? value = (subscriptionInfo.IsTrial ? subscriptionInfo.Subscription.TrialEnd : subscriptionInfo.Subscription.ExpiresAt);
		if (ExpiredMessageVisibility == Visibility.Visible)
		{
			if (value.HasValue && IsDayPassed(value.Value))
			{
				ExpirationDateText = "Expired";
			}
			else
			{
				ExpirationDateText = "Active";
			}
		}
		else
		{
			string value2 = (subscriptionInfo.Subscription.Autorenewal ? "Renews on" : "Expires on");
			ExpirationDateText = $"{value2} {value:d}";
		}
	}

	private static bool IsDayPassed(DateTime date)
	{
		return date.Date < DateTime.Today;
	}

	private bool SetCurrentPlanStackPanelVisibility(string subscriptionType)
	{
		try
		{
			switch (subscriptionType)
			{
			case "1":
				CurrentPlanStackPanelVisibility = Visibility.Visible;
				return true;
			case "0":
			case "2":
			case "3":
			case "4":
				CurrentPlanStackPanelVisibility = Visibility.Collapsed;
				return false;
			default:
				CurrentPlanStackPanelVisibility = Visibility.Collapsed;
				_logger?.Error("subscription_type = " + subscriptionType, "SetCurrentPlanStackPanelVisibility", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Account\\ExpandedAccountViewModel.cs", 611);
				return false;
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify(ex.Message);
			return false;
		}
	}

	private void ShowExpiredMessage(string message)
	{
		ExpiredMessageVisibility = Visibility.Visible;
		ExpiredMessageText = message;
	}

	private void SetCurrentPlanText(ISubscriptionInfo subscription)
	{
		try
		{
			if (subscription == null)
			{
				CurrentPlanText = "NextAI VIP Unlimited";
				return;
			}
			if (subscription.IsTrial)
			{
				CurrentPlanText = "Trial";
			}
			else if (subscription.Message != null && subscription.Message.ToLower().Equals("subscription not found"))
			{
				ExpiredMessageText = subscription.Message;
				CurrentPlanText = subscription.Message;
				ShowExpiredMessage("Subscription not found");
			}
			else
			{
				CurrentPlanText = !string.IsNullOrEmpty(subscription.Message) ? subscription.Message : "NextAI VIP Residential";
			}
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "SetCurrentPlanText", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Account\\ExpandedAccountViewModel.cs", 649);
			_bugsnagService.Notify(ex.Message);
		}
	}

	private void ExtendSubscriptionButtonClickCommandExecute(object obj)
	{
		switch (_expandedWindow.SdkObject.AccountTypeHelper.GetAccountType())
		{
		case AccountType.NextAiTechnology:
			ExtendNextAiTechnologySubscription();
			break;
		case AccountType.NextAiGlobal:
			ExtendNextAiGlobalSubscription();
			break;
		case AccountType.None:
			_logger?.Warning("No subscription found", "ExtendSubscriptionButtonClickCommandExecute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Account\\ExpandedAccountViewModel.cs", 665);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void ExtendNextAiGlobalSubscription()
	{
		_subscriptionFetchTimer.Start();
		_browserLinksOpener.OpenBrowserLink(VPNConstants.Links.SpSSubscriptionLink);
	}

	private void ExtendNextAiTechnologySubscription()
	{
		if (_subscriptionInfo.IsTrial)
		{
			if (RenewSubscriptionText.Equals("Turn on auto-renew"))
			{
				RenewNCSubscriptionAction();
				return;
			}
			_subscriptionFetchTimer.Start();
			_nextaitechnologySubscriptionProlongationHandler.Prolongate();
		}
		else
		{
			RenewNCSubscriptionAction();
		}
	}

	private void RenewNCSubscriptionAction()
	{
		_autoRenewalMessageBoxWindow.ShowDialog();
		if (_autoRenewalMessageBoxWindow.DialogResult)
		{
			_analyticsService.SendNotification("Account - Extend subscription");
			_subscriptionInfo.ToggleAutoRenewal();
			if (_expandedWindow.Mainpanel.SubscriptionExpireSoon.Visibility == Visibility.Visible)
			{
				_expandedWindow.Mainpanel.SubscriptionExpireSoon.Visibility = Visibility.Collapsed;
			}
			RenewSubscriptionTextVisibility = Visibility.Collapsed;
			ExpiredMessageVisibility = Visibility.Collapsed;
			AutoRenewalText = "ON";
			if (_subscriptionInfo?.Subscription != null)
			{
				_subscriptionInfo.Subscription.Autorenewal = true;
			}
			SetExpirationDateText(_subscriptionInfo);
		}
	}

	private void SignOutMouseDownCommandExecute(object obj)
	{
		_analyticsService.SendNotification("Account - Sign Out");
		_expandedWindow.SdkObject.TaskBarService.SignOutCommonFunction();
	}
}
