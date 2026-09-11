using System;
using System.Threading.Tasks;
using System.Windows;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Trial;

internal class TrialFirstChipsViewModel : ViewModelBase
{
	private readonly IVpnTypeDefiner _vpnTypeDefiner;

	private readonly IAccountTypeHelper _accountTypeHelper;

	private string _leftSideText;

	private string _rightSideText;

	private Visibility _controlVisibility = Visibility.Collapsed;

	private Visibility _middleTextVisibility;

	private readonly IAppLogger _logger;

	private int _trafficLeft;

	public string LeftSideText
	{
		get
		{
			return _leftSideText;
		}
		set
		{
			if (_leftSideText != value)
			{
				_leftSideText = value;
				OnPropertyChanged("LeftSideText");
			}
		}
	}

	public string RightSideText
	{
		get
		{
			return _rightSideText;
		}
		set
		{
			if (_rightSideText != value)
			{
				_rightSideText = value;
				OnPropertyChanged("RightSideText");
			}
		}
	}

	public Visibility ControlVisibility
	{
		get
		{
			return _controlVisibility;
		}
		set
		{
			if (_controlVisibility != value)
			{
				_controlVisibility = value;
				OnPropertyChanged("ControlVisibility");
			}
		}
	}

	public Visibility MiddleTextVisibility
	{
		get
		{
			return _middleTextVisibility;
		}
		set
		{
			if (_middleTextVisibility != value)
			{
				_middleTextVisibility = value;
				OnPropertyChanged("MiddleTextVisibility");
			}
		}
	}

	public TrialFirstChipsViewModel(IVpnTypeDefiner vpnTypeDefiner, IAccountTypeHelper accountTypeHelper, IAppLogger logger)
	{
		_vpnTypeDefiner = vpnTypeDefiner;
		_accountTypeHelper = accountTypeHelper;
		_logger = logger;
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
	}

	private void OnVpnModeChanged(VpnType obj)
	{
		switch (obj)
		{
		case VpnType.NextAiVPN:
			MiddleTextVisibility = Visibility.Visible;
			SetTexts();
			break;
		case VpnType.Streaming:
			MiddleTextVisibility = Visibility.Collapsed;
			LeftSideText = "TRIAL";
			RightSideText = string.Empty;
			break;
		default:
			throw new ArgumentOutOfRangeException("obj", obj, null);
		}
	}

	public Task RefreshData(ISubscriptionInfo subscriptionInfo)
	{
		try
		{
			if (!subscriptionInfo.IsTrial)
			{
				ControlVisibility = Visibility.Collapsed;
				return Task.CompletedTask;
			}
			if (subscriptionInfo.Subscription == null)
			{
				return Task.CompletedTask;
			}
			_trafficLeft = 30720 - subscriptionInfo.Subscription.Consumption;
			OnVpnModeChanged(_vpnTypeDefiner.DefineVpnType());
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "RefreshData", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Trial\\TrialFirstChipsViewModel.cs", 144);
		}
		return Task.CompletedTask;
	}

	private void SetTexts()
	{
		if (_trafficLeft <= 0)
		{
			ControlVisibility = Visibility.Collapsed;
		}
		else if (_trafficLeft > 2048)
		{
			LeftSideText = "TRIAL";
			RightSideText = $"{GetTrafficLeftGbValue(_trafficLeft)} GB Left";
		}
		else if (_trafficLeft < 1024)
		{
			LeftSideText = "LOW DATA";
			RightSideText = $"{_trafficLeft} MB Left";
		}
		else
		{
			LeftSideText = "LOW DATA";
			RightSideText = $"{GetTrafficLeftGbValue(_trafficLeft)} GB Left";
		}
	}

	private static int GetTrafficLeftGbValue(int trafficLeft)
	{
		return trafficLeft / 1024;
	}
}
