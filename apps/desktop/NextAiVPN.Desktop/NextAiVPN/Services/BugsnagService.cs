using System;
using Bugsnag;
using Bugsnag.Payload;
using NextAiVPN.Common;

namespace NextAiVPN.Services;

internal class BugsnagService : IBugsnagService
{
	private readonly IClient _client;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly Func<bool> _isNetworkAvailable;

	public BugsnagService(IClient client, IAppSettingsHelper appSettingsHelper, Func<bool> isNetworkAvailable)
	{
		_client = client ?? throw new ArgumentNullException("client");
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_isNetworkAvailable = isNetworkAvailable ?? throw new ArgumentNullException("isNetworkAvailable");
		_client.BeforeNotify(AttachUser);
	}

	public void StartSession()
	{
		_client.SessionTracking.CreateSession();
	}

	public void Notify(string message)
	{
		if (!string.IsNullOrEmpty(message) && !message.Contains("Unable to parse valid value, access granted for user") && _isNetworkAvailable())
		{
			_client.Notify(new ArgumentException(message));
		}
	}

	public void Notify(System.Exception exception)
	{
		if (exception != null && _isNetworkAvailable())
		{
			_client.Notify(exception);
		}
	}

	private void AttachUser(Report report)
	{
		string value = _appSettingsHelper.GetValue("nickname");
		if (!string.IsNullOrEmpty(value))
		{
			report.Event.User = new User
			{
				Id = value,
				Name = value
			};
		}
	}
}
