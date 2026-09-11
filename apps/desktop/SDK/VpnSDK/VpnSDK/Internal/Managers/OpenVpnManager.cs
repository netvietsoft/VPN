using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OSVersionHelper;
using VpnSDK.Common.Settings;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Helpers;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Extensions;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.OpenVpn;
using VpnSDK.Private.OpenVpn.Configuration;
using VpnSDK.Private.OpenVpn.Driver;
using VpnSDK.Private.OpenVpn.Enums;
using VpnSDK.Private.OpenVpn.Exceptions;

namespace VpnSDK.Internal.Managers;

internal class OpenVpnManager : IProtocolManager, IDisposable, IDriverManager
{
	private const string DriverCertificateIssuedTo = "Microsoft Windows Hardware Compatibility Publisher";

	private const string DriversFolderName = "Drivers";

	private readonly ILogger _logger;

	private readonly VpnSDK.DTO.OpenVpnConfiguration _openVpnConfiguration;

	private readonly ILogger _openvpnLogger;

	private DriverManager _driverManager;

	private OpenVpnConnection _openVpnConnection;

	private string _driverCertPath;

	private bool _isDisposed;

	private bool _isActive;

	private readonly bool _isWindows81OrLower;

	private readonly ApiManager _apiManager;

	public bool IsActive
	{
		get
		{
			OpenVpnConnection openVpnConnection = _openVpnConnection;
			if (openVpnConnection != null && !openVpnConnection.IsDisposed)
			{
				return _isActive;
			}
			return false;
		}
	}

	public Action UnexpectedDisconnect { get; set; }

	public bool IsDriverDetected => _driverManager?.IsInstalled ?? false;

	public Version DriverVersion => _driverManager?.Version;

	internal OpenVpnManager(ApiManager apiManager, VpnSDK.DTO.OpenVpnConfiguration openVpnConfiguration)
	{
		_logger = LogProvider.GetLogger("VpnSDK::OpenVPN");
		_openvpnLogger = LogProvider.GetLogger("VpnSDK::OpenVPN::Process");
		_apiManager = apiManager;
		_openVpnConfiguration = openVpnConfiguration;
		_isWindows81OrLower = !WindowsVersionHelper.IsWindows10;
		Tuple<DriverManager, string> tuple = CreateDriverManager();
		_driverManager = tuple.Item1;
		_driverCertPath = tuple.Item2;
		_logger?.LogInformation("Initialized.");
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			UnexpectedDisconnect = null;
			DisposeConnection();
			_isDisposed = true;
		}
	}

	public async Task Connect(Server server, IConnectionConfiguration connectionConfiguration, IUser user, SplitTunnelClientSettings splitTunnelClientSettings, DnsSettings dnsSettings, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!(connectionConfiguration is IOpenVpnConnectionConfiguration openvpnConnectionConfiguration))
		{
			throw new InvalidCastException("connectionConfiguration should be an instance of IOpenVpnConnectionConfiguration");
		}
		if (_openVpnConnection != null)
		{
			_openVpnConnection.DataReceived -= OpenVpnConnectionOnDataReceived;
			_openVpnConnection.Dispose();
			_openVpnConnection = null;
		}
		OpenVpnEnvironment openVpnEnvironment = new OpenVpnEnvironment
		{
			ApplicationDirectory = _openVpnConfiguration.OpenVpnDirectory,
			ExecutableName = _openVpnConfiguration.OpenVpnExecutableFileName
		};
		_logger?.LogDebug("Connect using OpenVPN dependencies from folder : " + openVpnEnvironment.ApplicationDirectory);
		string text;
		string text2;
		if (openvpnConnectionConfiguration.DoubleHopSettings != null && openvpnConnectionConfiguration.DoubleHopSettings.IsDoubleHopEnabled)
		{
			DoubleHopConfigurationResponse obj = await _apiManager.GetOpenVPNDoubleHopConfiguration(cancellationToken, openvpnConnectionConfiguration.DoubleHopSettings).ConfigureAwait(continueOnCapturedContext: false);
			text = obj.EntryServer;
			text2 = obj.Port.ToString();
			_logger?.LogInformation("Successfully retrieved double hop OpenVPN settings. Server and port have been configured accordingly.");
		}
		else
		{
			text = server.IP.ToString();
			text2 = openvpnConnectionConfiguration.Port.ToString();
		}
		OpenVpn openVpn = new OpenVpn(openVpnEnvironment, user?.VpnCredential, null, LogProvider.LoggerFactoryInstance);
		_openVpnConnection = openVpn.Connection();
		VpnSDK.Private.OpenVpn.Configuration.OpenVpnConfiguration openVpnConfiguration = new VpnSDK.Private.OpenVpn.Configuration.OpenVpnConfiguration(_openVpnConfiguration.ConfigurationFileOptions);
		openVpnConfiguration.Add("remote", new string[3]
		{
			text,
			text2,
			openvpnConnectionConfiguration.ProtocolType.ToString().ToLower()
		});
		openVpnConfiguration.Add("cipher", new string[1] { openvpnConnectionConfiguration.Cipher.Description() });
		VpnSDK.Private.OpenVpn.Configuration.OpenVpnConfiguration openVpnConfiguration2 = openVpnConfiguration;
		if (openvpnConnectionConfiguration.Scramble)
		{
			openVpnConfiguration2.Add("scramble", new string[2]
			{
				"obfuscate",
				server.Configuration.OpenVpn.ScramblePassphrase
			});
			_logger?.LogInformation("OpenVPN scramble switch added.");
		}
		List<string> list = new List<string>();
		if (dnsSettings.ShouldUpdateDns)
		{
			string[] array = dnsSettings.DnsServers ?? Array.Empty<string>();
			if (array.Length != 0)
			{
				openVpnConfiguration2.Add("pull-filter", new string[2] { "ignore", "\"dhcp-option DNS\"" });
				string[] array2 = array;
				foreach (string text3 in array2)
				{
					list.Add("--dhcp-option DNS " + text3);
				}
			}
		}
		if (splitTunnelClientSettings.IsDomainBasedSplitTunnelEnabled)
		{
			list.Add("--dhcp-option DNS " + IPAddress.Loopback.ToString());
		}
		if (list.Count > 0)
		{
			string text4 = list[0];
			int num = text4.IndexOf(" ", StringComparison.Ordinal);
			if (num != -1)
			{
				text4 = text4.Substring(num + 1);
				list[0] = text4;
			}
			openVpnConfiguration2.Add("dhcp-option", list.ToArray());
		}
		_openVpnConnection.DataReceived += OpenVpnConnectionOnDataReceived;
		_openVpnConnection.UnexpectedDisconnect += OpenVpnConnectionOnUnexpectedDisconnect;
		try
		{
			_logger?.LogInformation("Connecting to {Server}, Protocol={Protocol}, Port={Port}, Cipher={Cipher}, Scrambled={Scramble}", server.Hostname.Split(new char[1] { '.' })[0], openvpnConnectionConfiguration.ProtocolType, openvpnConnectionConfiguration.Port, openvpnConnectionConfiguration.Cipher.ToString().Replace('_', '-'), openvpnConnectionConfiguration.Scramble);
			await _openVpnConnection.Connect(openVpnConfiguration2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			_logger?.LogInformation("Successfully connected.");
			_isActive = true;
		}
		catch (OperationCanceledException)
		{
			DisposeConnection();
			throw;
		}
		catch (VpnSDK.Private.OpenVpn.Exceptions.AuthenticationException inner)
		{
			DisposeConnection();
			throw new VPNAuthenticationException("An authentication error occurred.", inner);
		}
		catch (Exception inner2)
		{
			DisposeConnection();
			throw new VpnException("Unable to connect to the VPN server.", inner2);
		}
	}

	public async Task Disconnect()
	{
		if (_openVpnConnection != null)
		{
			await _openVpnConnection.Disconnect().ConfigureAwait(continueOnCapturedContext: false);
			DisposeConnection();
		}
	}

	public async Task<DriverInstallResult> InstallDriver()
	{
		if (_driverManager != null)
		{
			if (_isWindows81OrLower)
			{
				try
				{
					InstallCertificate(_driverCertPath, "Microsoft Windows Hardware Compatibility Publisher");
				}
				catch
				{
				}
			}
			NetworkComponentFinder.Update();
			if (IsDriverDetected)
			{
				await _driverManager.Remove().ConfigureAwait(continueOnCapturedContext: false);
				NetworkComponentFinder.Update();
			}
			DevconReturnCode returnCode = await _driverManager.Install().ConfigureAwait(continueOnCapturedContext: false);
			NetworkComponentFinder.Update();
			switch (returnCode)
			{
			case DevconReturnCode.Success:
				try
				{
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Enum\\ROOT\\NET\\");
					string[] subKeyNames = registryKey.GetSubKeyNames();
					foreach (string name in subKeyNames)
					{
						try
						{
							RegistryKey registryKey2 = registryKey.OpenSubKey(name, writable: true);
							object value = registryKey2.GetValue("HardwareID", string.Empty);
							if ((value is string text && text == _driverManager.DetectedDriver.ToString()) || (value is string[] source && source.Any((string x) => x.Equals(_driverManager.DetectedDriver.ToString(), StringComparison.OrdinalIgnoreCase))))
							{
								registryKey2.SetValue("FriendlyName", _openVpnConfiguration.TapDeviceFriendlyName, RegistryValueKind.String);
								_logger?.LogInformation("Updated string.");
								break;
							}
						}
						catch
						{
						}
					}
				}
				catch
				{
					_logger?.LogWarning("Unable to open registry keys.");
				}
				return DriverInstallResult.Success;
			case DevconReturnCode.RebootRequired:
				_logger?.LogInformation("Driver Installation : Reboot required");
				return DriverInstallResult.RebootRequired;
			default:
				await _driverManager.Remove().ConfigureAwait(continueOnCapturedContext: false);
				_logger?.LogError("TAP device installation returned error code {ErrorCode}", (int)returnCode);
				return DriverInstallResult.Failed;
			}
		}
		_logger?.LogWarning("Driver Manager was not initialized.");
		return DriverInstallResult.Failed;
	}

	public async Task<DriverUninstallResult> RemoveDriver()
	{
		if (_driverManager == null)
		{
			_logger?.LogWarning("Driver Manager was not initialized.");
			return DriverUninstallResult.Failed;
		}
		if (IsDriverDetected)
		{
			DevconReturnCode devconReturnCode = await _driverManager.Remove().ConfigureAwait(continueOnCapturedContext: false);
			NetworkComponentFinder.Update();
			switch (devconReturnCode)
			{
			case DevconReturnCode.Success:
				return DriverUninstallResult.Success;
			case DevconReturnCode.RebootRequired:
				_logger?.LogInformation("Driver Uninstallation : Reboot required");
				return DriverUninstallResult.RebootRequired;
			default:
				_logger?.LogError("TAP device uninstallation returned error code {ErrorCode}", (int)devconReturnCode);
				return DriverUninstallResult.Failed;
			}
		}
		_logger?.LogWarning("Driver not detected.");
		return DriverUninstallResult.NotAvailable;
	}

	public async Task DisposeAsync()
	{
		await Task.Run(delegate
		{
			Dispose();
		}).ConfigureAwait(continueOnCapturedContext: false);
	}

	private void InstallCertificate(string cerFileName, string expectedIssuedTo)
	{
		X509Certificate2 x509Certificate = new X509Certificate2(cerFileName);
		if (x509Certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false).Trim().Equals(expectedIssuedTo.Trim()))
		{
			X509Store x509Store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
			x509Store.Open(OpenFlags.ReadWrite);
			x509Store.Add(x509Certificate);
			x509Store.Close();
			_logger?.LogInformation("OpenVPN cert installed.");
		}
	}

	private Tuple<DriverManager, string> CreateDriverManager()
	{
		if (_openVpnConfiguration != null)
		{
			try
			{
				string openVpnDriverDirectory = _openVpnConfiguration.OpenVpnDriverDirectory;
				string item = Path.Combine(openVpnDriverDirectory, "..\\driver_publisher.cer");
				DriverManager driverManager = new DriverManager(openVpnDriverDirectory, _openVpnConfiguration.PreferredTapAdapter, null, LogProvider.LoggerFactoryInstance);
				OnOpenVpnDriverUpdate(driverManager.DetectedDriver);
				driverManager.OnDriverUpdated += OnOpenVpnDriverUpdate;
				return new Tuple<DriverManager, string>(driverManager, item);
			}
			catch (Exception exception)
			{
				_logger?.LogError(exception, "Unable to load TAP manager.");
			}
		}
		return new Tuple<DriverManager, string>(null, null);
	}

	private void OnOpenVpnDriverUpdate(TapDriver driver)
	{
		_openVpnConfiguration.SetDefaultOpenVpnDirectory(driver);
	}

	private void DisposeConnection()
	{
		_isActive = false;
		if (_openVpnConnection != null)
		{
			_openVpnConnection.DataReceived -= OpenVpnConnectionOnDataReceived;
			_openVpnConnection.UnexpectedDisconnect -= OpenVpnConnectionOnUnexpectedDisconnect;
			_openVpnConnection.Dispose();
			_openVpnConnection = null;
		}
	}

	private void OpenVpnConnectionOnDataReceived(string obj)
	{
		_openvpnLogger.LogInformation(obj);
	}

	private void OpenVpnConnectionOnUnexpectedDisconnect()
	{
		DisposeConnection();
		UnexpectedDisconnect?.Invoke();
	}
}
