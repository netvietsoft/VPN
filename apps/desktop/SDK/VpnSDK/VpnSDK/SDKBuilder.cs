using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Helpers;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Configuration;
using VpnSDK.Internal.Helpers;

namespace VpnSDK;

public class SDKBuilder<T> where T : ISDK
{
	private string _apiKey;

	private string[] _apiBaseUrl;

	private string _loginApiUrl;

	private string _brandingKey;

	private string _applicationName;

	private string _authenticationToken;

	private TimeSpan? _serverListCache;

	private bool _runInUserspace;

	private SynchronizationContext _synchronizationContext;

	private bool _handleAutomaticTokenRefreshing = true;

	private bool _buildMockup;

	private RasConfiguration _rasConfiguration;

	private bool _useTokenAuthentication;

	private string _calloutDriverName;

	private string _serverCacheDirectory;

	private Dictionary<NetworkConnectionType, bool> _availableVpnTypes = new Dictionary<NetworkConnectionType, bool>
	{
		{
			NetworkConnectionType.OpenVPN,
			true
		},
		{
			NetworkConnectionType.IKEv2,
			true
		},
		{
			NetworkConnectionType.WireGuard,
			true
		}
	};

	private OpenVpnConfiguration _openVpnConfiguration;

	private WireguardConfiguration _wireguardConfiguration;

	public SDKBuilder<T> SetOpenVpnConfiguration(OpenVpnConfiguration value)
	{
		_openVpnConfiguration = value;
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public SDKBuilder<T> SetWireguardConfiguration(WireguardConfiguration value)
	{
		_wireguardConfiguration = value;
		return this;
	}

	public SDKBuilder<T> SetApiKey(string value)
	{
		_apiKey = value?.Trim();
		return this;
	}

	public SDKBuilder<T> SetBrandingKey(string value)
	{
		_brandingKey = value?.Trim();
		return this;
	}

	public SDKBuilder<T> SetSynchronizationContext(SynchronizationContext context)
	{
		_synchronizationContext = context;
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public SDKBuilder<T> SetApiBaseUrl(params string[] value)
	{
		_apiBaseUrl = value;
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public SDKBuilder<T> SetRunInUserspace(bool runInUserspace)
	{
		_runInUserspace = runInUserspace;
		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public SDKBuilder<T> SetBuildMockup(bool value)
	{
		_buildMockup = value;
		return this;
	}

	public SDKBuilder<T> SetApplicationName(string value)
	{
		_applicationName = value;
		return this;
	}

	public SDKBuilder<T> SetAuthenticationToken(string value)
	{
		value = value.TrimStart(new char[1] { '@' });
		_authenticationToken = value.Trim();
		return this;
	}

	public SDKBuilder<T> SetRasConfiguration(RasConfiguration value)
	{
		_rasConfiguration = value;
		return this;
	}

	public SDKBuilder<T> SetServerListCache(TimeSpan? time)
	{
		_serverListCache = time;
		return this;
	}

	[Obsolete("This method is obsolete. The SDK now handles automatic token refreshing by default.")]
	public SDKBuilder<T> SetAutomaticRefreshTokenHandling(bool automaticallyRefresh)
	{
		_handleAutomaticTokenRefreshing = automaticallyRefresh;
		return this;
	}

	public SDKBuilder<T> SetTokenBasedAuthentication(bool useTokenAuthentication)
	{
		_useTokenAuthentication = useTokenAuthentication;
		return this;
	}

	public SDKBuilder<T> SetCalloutDriverName(string calloutDriverName)
	{
		_calloutDriverName = calloutDriverName;
		return this;
	}

	public T Create()
	{
		if (typeof(T).Equals(typeof(ISDKInternal)))
		{
			string text = HashHelper.SHA256HashString(_apiKey).Substring(0, 32);
			if (text != "580a3d5ab4bf2c4ba73f894a210a3bdc" && text != "bc43203ca87568b81e82246ac06fe7d2")
			{
				throw new NotSupportedException("ISDKInternal is not supported, use ISDK interface instead.");
			}
		}
		if (_openVpnConfiguration == null)
		{
			_openVpnConfiguration = new OpenVpnConfiguration();
		}
		if (_rasConfiguration == null)
		{
			_rasConfiguration = new RasConfiguration
			{
				RasDeviceDescription = _applicationName
			};
		}
		if (_wireguardConfiguration == null)
		{
			_wireguardConfiguration = new WireguardConfiguration
			{
				ConnectionName = _applicationName
			};
		}
		Validate();
		SDKConfiguration sDKConfiguration = new SDKConfiguration
		{
			AvailableVpnTypes = _availableVpnTypes,
			RasConfiguration = _rasConfiguration,
			ApiKey = _apiKey,
			BrandingKey = _brandingKey,
			ApplicationName = _applicationName,
			AuthorizationToken = _authenticationToken,
			KeepServerCache = _serverListCache,
			OpenVpnConfiguration = _openVpnConfiguration,
			WireguardConfiguration = _wireguardConfiguration,
			AutomaticRefreshTokenHandling = _handleAutomaticTokenRefreshing,
			SDKType = typeof(T),
			SynchronizationContext = _synchronizationContext,
			RunInUserspace = _runInUserspace,
			UseTokenAuthentication = _useTokenAuthentication,
			CalloutDriverName = _calloutDriverName,
			ServerListCacheDirectory = _serverCacheDirectory
		};
		if (_apiBaseUrl != null && _apiBaseUrl.Length != 0)
		{
			sDKConfiguration.ApiBaseUrls = _apiBaseUrl;
		}
		if (!string.IsNullOrEmpty(_loginApiUrl))
		{
			sDKConfiguration.LoginApiUrl = _loginApiUrl;
		}
		if (string.IsNullOrEmpty(sDKConfiguration.ServerListCacheDirectory))
		{
			sDKConfiguration.ServerListCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _applicationName);
		}
		if (_buildMockup)
		{
			return (T)(object)new SDKCoreMockup(sDKConfiguration);
		}
		return (T)(object)new SDKCore(sDKConfiguration);
	}

	public void Validate()
	{
		List<string> list = new List<string>();
		if (_brandingKey == null)
		{
			if (_apiKey == null)
			{
				list.Add("API Key");
			}
			if (_authenticationToken == null && _apiBaseUrl == null)
			{
				list.Add("Authentication Token");
			}
		}
		if (string.IsNullOrEmpty(_applicationName))
		{
			list.Add("Application Name");
		}
		if (!string.IsNullOrEmpty(_applicationName) && string.IsNullOrEmpty(StringHelper.RemoveSpecialCharactersFromString(_applicationName)))
		{
			throw new InvalidConfigurationException("The application name cannot consist only of special characters. Please use at least one alphanumeric character in the application name.");
		}
		if (list.Count > 0)
		{
			throw new InvalidConfigurationException("The following mandatory parameters were not set. " + string.Join(", ", list));
		}
		if (!string.IsNullOrEmpty(_calloutDriverName) && !StringHelper.IsCalloutDriverNameValid(_calloutDriverName))
		{
			throw new InvalidConfigurationException("Callout driver should only contain valid characters.");
		}
		VpnProtocolDiagnostics.DiagnoseProtocolsAvailability(_openVpnConfiguration, _rasConfiguration, _wireguardConfiguration, _availableVpnTypes);
		if (_runInUserspace)
		{
			if (_availableVpnTypes.ContainsKey(NetworkConnectionType.OpenVPN))
			{
				_availableVpnTypes[NetworkConnectionType.OpenVPN] = false;
			}
			if (_availableVpnTypes.ContainsKey(NetworkConnectionType.WireGuard))
			{
				_availableVpnTypes[NetworkConnectionType.WireGuard] = false;
			}
		}
	}

	private void UpdateFeatures<TInterface>()
	{
		if (typeof(TInterface).Name == "ISDKInternal" && !_availableVpnTypes.ContainsKey(NetworkConnectionType.WireGuard))
		{
			_availableVpnTypes.Add(NetworkConnectionType.WireGuard, value: true);
		}
	}
}
