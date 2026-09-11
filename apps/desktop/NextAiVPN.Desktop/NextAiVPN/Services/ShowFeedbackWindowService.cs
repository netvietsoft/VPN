using System;
using System.Globalization;
using System.Windows;
using NextAiVPN.Common;
using NextAiVPN.UI.ForQA;

namespace NextAiVPN.Services;

public class ShowFeedbackWindowService : IShowFeedbackWindowService
{
	private const string ConnectionCounter = "ConnectionCounter";

	private const string SendFeedbackCounter = "SendFeedbackCounter";

	private const string FirstRunDate = "FirstRunDate";

	private const string LastFeedbackSent = "LastFeedbackSent";

	private const string ConnectionsToShowFeedbackCounter = "ConnectionsToShowFeedbackCounter";

	private const string LastFeedbackClosed = "LastFeedbackClosed";

	private readonly int _minDays;

	private readonly int _maxDays;

	private DateTime _firstRunDate;

	private DateTime _lastFeedbackSentDate;

	private DateTime _feedbackClosedDate;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public ShowFeedbackWindowService(IAppSettingsHelper appSettingsHelper)
	{
		_appSettingsHelper = appSettingsHelper;
		_firstRunDate = GetDateValue("FirstRunDate");
		_minDays = int.Parse(_appSettingsHelper.GetValue("MinDays"), CultureInfo.InvariantCulture);
		_maxDays = int.Parse(_appSettingsHelper.GetValue("MaxDays"), CultureInfo.InvariantCulture);
	}

	public bool NeedToShow()
	{
		_lastFeedbackSentDate = GetDateValue("LastFeedbackSent");
		_feedbackClosedDate = GetDateValue("LastFeedbackClosed");
		_firstRunDate = GetDateValue("FirstRunDate");
		return IsNeedToShowFeedbackWindow();
	}

	public bool NeedToShowQA()
	{
		_lastFeedbackSentDate = GetDateValue("LastFeedbackSent");
		_feedbackClosedDate = GetDateValue("LastFeedbackClosed");
		_firstRunDate = GetDateValue("FirstRunDate");
		ShowQAWindow();
		return IsNeedToShowFeedbackWindow();
	}

	private bool IsNeedToShowFeedbackWindow()
	{
		if (!IsNeedToShowPopUp())
		{
			return false;
		}
		if (!IsFeedbackSend() && !FirstCondition() && !SecondCondition())
		{
			return SendFeedbackLoop();
		}
		return true;
	}

	private bool IsNeedToShowPopUp()
	{
		string value = _appSettingsHelper.GetValue("IsNeedToShowFeedbackPopUp");
		if (string.IsNullOrEmpty(value))
		{
			return true;
		}
		return value.Equals("1");
	}

	private bool SendFeedbackLoop()
	{
		if (GetValue("IsFeedbackDontSendAndClosedManually") == 1 && _feedbackClosedDate != DateTime.MinValue && IsTheDaysLeftAfterSkipSendFeedback())
		{
			int num = Math.Abs((DateTime.Now - _feedbackClosedDate).Days);
			if (num % 15 == 0 && num > 14)
			{
				return true;
			}
		}
		if (GetValue("SendFeedbackCounter") <= 0)
		{
			if (!IsTheDaysLeftAfterSkipSendFeedback())
			{
				return false;
			}
			int num2 = Math.Abs((DateTime.Now - _firstRunDate).Days) - 2 - 15;
			if (num2 < 14)
			{
				return false;
			}
			if (num2 % 15 == 0)
			{
				return true;
			}
			return false;
		}
		return IsFeedbackSend();
	}

	private bool IsTheDaysLeftAfterSkipSendFeedback()
	{
		return GetValue("ConnectionsAfterSkipCounter") > 2;
	}

	private bool IsFeedbackSend()
	{
		if (_lastFeedbackSentDate == DateTime.MinValue)
		{
			return false;
		}
		if (GetValue("SendFeedbackCounter") <= 0)
		{
			return false;
		}
		if (GetValue("ConnectionsToShowFeedbackCounter") < 3)
		{
			return false;
		}
		TimeSpan timeSpan = DateTime.Now - _lastFeedbackSentDate;
		if (Math.Abs(timeSpan.Days) != 0 && Math.Abs(timeSpan.Days) % 45 == 0)
		{
			return true;
		}
		return false;
	}

	private void ShowQAWindow()
	{
		QADaysLeftSetUpWindow qADaysLeftSetUpWindow = new QADaysLeftSetUpWindow();
		qADaysLeftSetUpWindow.ShowDialog();
		if (qADaysLeftSetUpWindow.Days != 0)
		{
			_firstRunDate = _firstRunDate.AddDays(qADaysLeftSetUpWindow.Days * -1);
		}
		if (qADaysLeftSetUpWindow.DaysLeftAfterUserCloseFeedbackWindow != 0)
		{
			if (_feedbackClosedDate == DateTime.MinValue)
			{
				MessageBox.Show("Please do the close feedback window manually");
			}
			else
			{
				_feedbackClosedDate = _feedbackClosedDate.AddDays(qADaysLeftSetUpWindow.DaysLeftAfterUserCloseFeedbackWindow * -1);
			}
		}
		if (qADaysLeftSetUpWindow.ConsiderFeedback && qADaysLeftSetUpWindow.DaysFeedback != DateTime.MinValue)
		{
			_lastFeedbackSentDate = qADaysLeftSetUpWindow.DaysFeedback;
		}
	}

	private bool FirstCondition()
	{
		if (_appSettingsHelper.GetValue("IsFeedbackSendExample1").Equals("1"))
		{
			return false;
		}
		if (Math.Abs((DateTime.Now - _firstRunDate).Days) < 2)
		{
			return false;
		}
		int value = GetValue("SendFeedbackCounter");
		int value2 = GetValue("ConnectionCounter");
		if (value == 0 && value2 >= 3)
		{
			_appSettingsHelper.SetValue("IsFeedbackSendExample1", "1");
			return true;
		}
		return false;
	}

	private bool SecondCondition()
	{
		if (_appSettingsHelper.GetValue("IsFeedbackSendExample2").Equals("1"))
		{
			return false;
		}
		if (Math.Abs((DateTime.Now - _firstRunDate).Days) <= _minDays + 2)
		{
			return false;
		}
		int value = GetValue("SendFeedbackCounter");
		int value2 = GetValue("ConnectionCounter");
		if (value == 0 && value2 >= 6)
		{
			_appSettingsHelper.SetValue("IsFeedbackSendExample2", "1");
			return true;
		}
		return false;
	}

	private bool ThirdCondition()
	{
		if (_appSettingsHelper.GetValue("IsFeedbackSendExample3").Equals("1"))
		{
			return false;
		}
		_ = _lastFeedbackSentDate == DateTime.MinValue;
		if (Math.Abs((DateTime.Now - _lastFeedbackSentDate).Days) < _maxDays)
		{
			return false;
		}
		if (Math.Abs((DateTime.Now - _firstRunDate).Days) <= _maxDays)
		{
			return false;
		}
		int value = GetValue("SendFeedbackCounter");
		int value2 = GetValue("ConnectionCounter");
		if (value > 0 && value2 >= 9)
		{
			_appSettingsHelper.SetValue("IsFeedbackSendExample3", "1");
			return true;
		}
		return false;
	}

	private int GetValue(string propertyName)
	{
		int.TryParse(_appSettingsHelper.GetValue(propertyName), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
		return result;
	}

	private DateTime GetDateValue(string propertyName)
	{
		DateTime.TryParse(_appSettingsHelper.GetValue(propertyName), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
		return result;
	}
}
