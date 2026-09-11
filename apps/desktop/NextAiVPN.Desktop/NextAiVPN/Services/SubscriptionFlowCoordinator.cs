using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using NextAiVPN.Common;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class SubscriptionFlowCoordinator : ISubscriptionFlowCoordinator
{
	private const string InactiveSubscriptionType = "0";

	private const string CompletePurchaseControl = "CompletePurchase";

	private const string MainSubscriptionControl = "MainSubscription";

	private readonly ISubscriptionInfo _subscriptionInfo;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private DispatcherTimer _limitTimer;

	public event Action TrialFirstWindowRequested;

	public event Action<TrialChipsUpdate> TrialChipsUpdateRequested;

	public SubscriptionFlowCoordinator(ISubscriptionInfo subscriptionInfo, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_subscriptionInfo = subscriptionInfo ?? throw new ArgumentNullException("subscriptionInfo");
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
	}

	public async Task<bool> IsSubscriptionActiveAsync()
	{
		try
		{
			await _subscriptionInfo.RefreshData();
			if (_subscriptionInfo.Subscription == null)
			{
				await _subscriptionInfo.RefreshData();
			}
			if (_subscriptionInfo.IsTrialLimitReached)
			{
				TrialFirstWindowRequested?.Invoke();
				return false;
			}
			return _subscriptionInfo.SubscriptionType != null && !_subscriptionInfo.SubscriptionType.Equals("0");
		}
		catch (Exception ex)
		{
			_logger?.Error(ex, "IsSubscriptionActiveAsync", "SubscriptionFlowCoordinator.cs", 50);
			return true;
		}
	}

	public async Task ShowTrialFirstIfNeededAsync()
	{
		if (_subscriptionInfo.Subscription == null)
		{
			return;
		}
		if (_subscriptionInfo.IsTrialLimitReached)
		{
			TrialChipsUpdateRequested?.Invoke(TrialChipsUpdate.Hide);
			TrialFirstWindowRequested?.Invoke();
			return;
		}
		TrialChipsUpdateRequested?.Invoke(TrialChipsUpdate.ShowAndRefresh);
		if (_appSettingsHelper.GetValue("IsTrialFirstWindowShowed").Equals("1"))
		{
			if (_subscriptionInfo.IsTrial)
			{
				StartLimitTimer();
			}
		}
		else if (_subscriptionInfo.IsTrial)
		{
			_appSettingsHelper.SetValue("IsTrialFirstWindowShowed", "1");
			TrialFirstWindowRequested?.Invoke();
			StartLimitTimer();
		}
	}

	public string GetNoSubscriptionControl()
	{
		if (!_appSettingsHelper.GetValue("IsPurchaseStarted").Equals("1"))
		{
			return "MainSubscription";
		}
		return "CompletePurchase";
	}

	private void StartLimitTimer()
	{
		if (_limitTimer == null)
		{
			_limitTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromHours(1)
			};
			_limitTimer.Tick += async delegate
			{
				try
				{
					await OnLimitTimerTickAsync();
				}
				catch (Exception exception)
				{
					_logger?.Error(exception, "StartLimitTimer", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\SubscriptionFlowCoordinator.cs", 139);
				}
			};
		}
		_limitTimer.Stop();
		_limitTimer.Start();
	}

	internal async Task OnLimitTimerTickAsync()
	{
		if (_appSettingsHelper.GetValue("IsLoggedIn").Equals("0"))
		{
			_limitTimer?.Stop();
			return;
		}
		await _subscriptionInfo.RefreshData();
		TrialChipsUpdateRequested?.Invoke(TrialChipsUpdate.Refresh);
		if (_subscriptionInfo.IsTrialLimitReached)
		{
			_limitTimer?.Stop();
			TrialChipsUpdateRequested?.Invoke(TrialChipsUpdate.Hide);
			TrialFirstWindowRequested?.Invoke();
		}
	}
}
