using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using VpnSDK.Common.Settings;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Private.API;

internal class VpnApiExtended : VpnApi
{
	private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

	private AsyncRetryPolicy _retryPolicy;

	private User _user;

	private User User
	{
		get
		{
			return _user;
		}
		set
		{
			UpdateUserDetails(value);
			RaiseTokenRefreshedEvent(_user);
		}
	}

	public event EventHandler<User> OnTokenRefresh;

	public VpnApiExtended(string apiKey, string brandToken = null, string baseUrl = "https://api.wlvpn.com/v3", TimeSpan timeout = default(TimeSpan), ILoggerFactory loggerFactory = null)
		: this(apiKey, brandToken, timeout, null, loggerFactory, new Uri(baseUrl.TrimEnd('/')))
	{
	}

	public VpnApiExtended(string apiKey, string brandToken, TimeSpan timeout, string loginUrl, ILoggerFactory loggerFactory, params string[] baseUrls)
		: this(apiKey, brandToken, timeout, (!string.IsNullOrEmpty(loginUrl)) ? new Uri(loginUrl.TrimEnd('/')) : null, loggerFactory, baseUrls.Select((string x) => new Uri(x.TrimEnd('/'))).ToArray())
	{
	}

	public VpnApiExtended(string apiKey, string brandToken, TimeSpan timeout, Uri loginUrl, ILoggerFactory loggerFactory, params Uri[] baseUrls)
		: base(apiKey, brandToken, timeout, loginUrl, loggerFactory, baseUrls)
	{
		InitRetryPolicy();
		VpnApi._logger?.LogInformation("VpnApiExtended in local");
	}

	internal void SetTokenExpire(DateTime expireDateTime)
	{
		if (expireDateTime.Kind != DateTimeKind.Utc)
		{
			throw new ArgumentException("ExpireDateTime must be in UTC format");
		}
		if (_user != null)
		{
			_user.AccessTokenExpiry = expireDateTime;
			RaiseTokenRefreshedEvent(_user);
		}
	}

	public void SetApiTimeout(TimeSpan timespan)
	{
		_httpApiHandler?.SetApiTimeout(timespan);
	}

	public override async Task<GeoIP> GetGeolocation(CancellationToken cancellationToken = default(CancellationToken), bool oneTime = false)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetGeolocation(cancellationToken, oneTime), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<List<string>> GetIkev2ProtocolConfig(CancellationToken cancellationToken)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetIkev2ProtocolConfig(cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<NetworkCredentialResponse> GetIKEv2VpnTokenCredentials(string server, CancellationToken cancellationToken)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetIKEv2VpnTokenCredentials(server, cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<AccountMetadataResponse> SaveAccountMetadata(Dictionary<string, string> keyValuePairs, CancellationToken cancellationToken = default(CancellationToken))
	{
		return await ExecuteWithTokenExpirationHandling(() => base.SaveAccountMetadata(keyValuePairs, cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<AccountMetadataResponse> GetAccountMetadata(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetAccountMetadata(cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<Network> GetServers(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetServers(cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<Tuple<bool, Network>> GetServersIfUpdated(CancellationToken cancellationToken = default(CancellationToken), bool ignoreCache = false)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetServersIfUpdated(cancellationToken, ignoreCache), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<WireguardConfiguration> GetWireGuardConfiguration(CancellationToken cancellationToken, string hostname, string configUuid, string publicKey = null, string privateKey = null, bool allowLan = true)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetWireGuardConfiguration(cancellationToken, hostname, configUuid, publicKey, privateKey, allowLan), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<WireguardConfiguration> GetWireGuardConfiguration(CancellationToken cancellationToken, string hostname, string configUuid, DoubleHopSettings doubleHopSettings, string publicKey = null, string privateKey = null, bool allowLan = true)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetWireGuardConfiguration(cancellationToken, hostname, configUuid, doubleHopSettings, publicKey, privateKey, allowLan), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<User> Login(string username, string password, CancellationToken cancellationToken = default(CancellationToken))
	{
		return User = await base.Login(username, password, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<User> LoginWithAuthTokens(string accessToken, string refreshToken, CancellationToken cancellationToken = default(CancellationToken))
	{
		User user = await base.LoginWithAuthTokens(accessToken, refreshToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (user.AccessToken.Equals(accessToken, StringComparison.InvariantCultureIgnoreCase))
		{
			UpdateUserDetails(user);
		}
		else
		{
			User = user;
		}
		return user;
	}

	public override async Task<string> Ping(CancellationToken cancellationToken = default(CancellationToken))
	{
		return await ExecuteWithTokenExpirationHandling(() => base.Ping(cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<bool> Logout(CancellationToken cancellationToken = default(CancellationToken))
	{
		bool num = await base.Logout(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (num)
		{
			User = null;
		}
		return num;
	}

	public override async Task<User> RefreshToken(User user, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		await _semaphore.WaitAsync(TimeSpan.FromSeconds(15.0), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			User user2 = _user;
			if (user2 != null && !user2.HasValidToken)
			{
				VpnApi._logger?.LogWarning("User token is invalid. So, refreshing the tokens...");
				User = await base.RefreshToken(user, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				VpnApi._logger?.LogInformation("Completed token refresh.");
			}
			else
			{
				VpnApi._logger?.LogInformation("Skipped token refresh request, as the current tokens are still valid.");
			}
			return _user;
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public override async Task<AdditionalConfigResponse> GetAdditionalConfig(CancellationToken cancellationToken)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetAdditionalConfig(cancellationToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task<DoubleHopConfigurationResponse> GetOpenVPNDoubleHopConfiguration(CancellationToken cancellationToken, DoubleHopSettings doubleHopSettings)
	{
		return await ExecuteWithTokenExpirationHandling(() => base.GetOpenVPNDoubleHopConfiguration(cancellationToken, doubleHopSettings), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private void RaiseTokenRefreshedEvent(User user)
	{
		OnTokenRefresh?.Invoke(this, user);
	}

	private void InitRetryPolicy()
	{
		_retryPolicy = Policy.Handle((VpnApiException ex) => ex.Error == ApiError.AccessTokenExpired).WaitAndRetryAsync(1, (int retryCount) => TimeSpan.FromMilliseconds(100.0), async delegate(Exception ex, TimeSpan retryCount)
		{
			if (_user != null)
			{
				VpnApi._logger?.LogWarning("API call request failed with error \"" + ex.Message + "\". So, Refreshing the token...");
				_user.AccessTokenExpiry = DateTime.UtcNow;
				await RefreshToken(_user).ConfigureAwait(continueOnCapturedContext: false);
				VpnApi._logger?.LogInformation("Token refresh completed.");
			}
			else
			{
				VpnApi._logger?.LogWarning("Cannot refresh token as the user object is null.");
			}
		});
	}

	private async Task<T> ExecuteWithTokenExpirationHandling<T>(Func<Task<T>> apiMethod, CancellationToken cancellationToken)
	{
		if (_user != null)
		{
			await RefreshToken(_user, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => apiMethod(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private void UpdateUserDetails(User value)
	{
		_user = value;
		if (_user != null && _apiType == ApiType.WLVPN && base.UseTokenAuthentication)
		{
			_user.Username = value.Email;
			_user.VpnCredential = new NetworkCredential(_user.GetUsernameWithBrand(value.Email, value.Brand), value.AccessToken);
		}
	}
}
