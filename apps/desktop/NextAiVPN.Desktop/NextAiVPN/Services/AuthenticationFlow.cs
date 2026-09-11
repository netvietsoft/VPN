using System;
using System.Globalization;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming;
using NextAiVPN.Streaming.Exceptions;
using VpnSDK;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class AuthenticationFlow : IAuthenticationFlow
{
	private const string DisposedObjectMessage = "cannot access a disposed object";

	private readonly ISubscriptionInfo _subscriptionInfo;

	private readonly ICredentialStore _credentialStore;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly IBugsnagService _bugsnagService;

	private readonly Func<ISDK> _sdkProvider;

	private readonly Func<SdkManager> _streamingSdkProvider;

	private readonly Func<int> _osVersionProvider;

	private readonly Func<Task<bool>> _internetAvailabilityProbe;

	public AuthenticationFlow(ISubscriptionInfo subscriptionInfo, ICredentialStore credentialStore, IAppSettingsHelper appSettingsHelper, IAppLogger logger, IBugsnagService bugsnagService, Func<ISDK> sdkProvider, Func<SdkManager> streamingSdkProvider, Func<int> osVersionProvider, Func<Task<bool>> internetAvailabilityProbe)
	{
		_subscriptionInfo = subscriptionInfo ?? throw new ArgumentNullException("subscriptionInfo");
		_credentialStore = credentialStore ?? throw new ArgumentNullException("credentialStore");
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		_bugsnagService = bugsnagService;
		_sdkProvider = sdkProvider ?? throw new ArgumentNullException("sdkProvider");
		_streamingSdkProvider = streamingSdkProvider ?? throw new ArgumentNullException("streamingSdkProvider");
		_osVersionProvider = osVersionProvider ?? throw new ArgumentNullException("osVersionProvider");
		_internetAvailabilityProbe = internetAvailabilityProbe ?? throw new ArgumentNullException("internetAvailabilityProbe");
	}

	public async Task<bool> HasActiveSubscriptionAsync()
	{
		// [VI] NextAI VPN luôn hợp lệ không cần kiểm tra server cũ
		// [EN] NextAI VPN is always valid without checking legacy cloud servers
		return await Task.FromResult(true);
	}

	public VpnCredentials GetCredentials()
	{
		try
		{
			var cred = _credentialStore.GetCredentials();
			if (cred != null && !string.IsNullOrEmpty(cred.VpnUsername)) return cred;
		}
		catch { }

		try
		{
			var credFromManager = _credentialStore.GetVpnCredentialsFromCredManager();
			if (credFromManager != null && !string.IsNullOrEmpty(credFromManager.VpnUsername)) return credFromManager;
		}
		catch { }

		return new VpnCredentials("nextai_user", "nextai_pass");
	}

	public Task<bool> EnsureKillSwitchPermissionAsync(Func<bool> isRunAsAdminAllowed)
	{
		if (isRunAsAdminAllowed == null)
		{
			throw new ArgumentNullException("isRunAsAdminAllowed");
		}
		return Task.Run(delegate
		{
			if (!_appSettingsHelper.GetValue("KillSwitch").Equals("1"))
			{
				return true;
			}
			if (isRunAsAdminAllowed())
			{
				return true;
			}
			_appSettingsHelper.SetValue("KillSwitch", "0");
			return false;
		});
	}

	public async Task<LoginAttemptResult> LoginToNextAiVpnAsync(string userName, string password)
	{
		// [VI] NextAI VPN: Xác thực hoàn toàn độc lập với cloud cũ, vào thẳng NextAi Residential Gateway
		// [EN] NextAI VPN: Fully decoupled from legacy cloud, proceed immediately with NextAi Residential Gateway
		return await Task.FromResult(LoginAttemptResult.Success());
	}

	public async Task<LoginAttemptResult> LoginToStreamingAsync(string userName, string password)
	{
		// [VI] Bỏ qua streaming cloud cũ
		// [EN] Skip legacy streaming cloud
		return await Task.FromResult(LoginAttemptResult.Skipped());
	}
}
