using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OSVersionHelper;
using VpnSDK.Common.Settings;
using VpnSDK.DTO;
using VpnSDK.Helpers;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Helpers;
using VpnSDK.Internal.WireGuard;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Internal.Managers;

internal class WireGuardManager : IProtocolManager, IDisposable
{
	private const int PingAfter = 5;

	private const int NoDataTimeout = 15;

	private const int ConnectionProcessTimeout = 30;

	private const int InterfacePullCooldown = 1000;

	private const int AdapterNotFoundErrorCode = -2147467259;

	private const string SignaturesUnmanagedRegistryPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\NetworkList\\Signatures\\Unmanaged";

	private const string ProfilesRegistryPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\NetworkList\\Profiles";

	private readonly ILogger _logger;

	private bool _isWindows7;

	private readonly ApiManager _apiManager;

	private string _configFile;

	private VpnSDK.Private.API.Wireguard.DTO.WireguardConfiguration _wgConfig;

	private volatile bool _connected;

	private readonly VpnSDK.DTO.WireguardConfiguration _configuration;

	private string _wireGuardLogFile;

	private readonly Ringlogger _ringLogger;

	private readonly Thread _wireGuardLogsThread;

	private Thread _wgInterfaceThread;

	private ManualResetEventSlim _wgInterfaceThreadStop = new ManualResetEventSlim(initialState: false);

	private ManualResetEventSlim _logsThreadStop = new ManualResetEventSlim(initialState: false);

	private TaskCompletionSource<bool> _connectionHandled;

	private int _currentSuffix;

	public bool IsActive => _connected;

	public Action UnexpectedDisconnect { get; set; }

	internal WireGuardManager(ApiManager apiManager, VpnSDK.DTO.WireguardConfiguration configuration)
	{
		_logger = LogProvider.GetLogger("VpnSDK::WireGuard");
		_apiManager = apiManager;
		_configuration = configuration;
		_isWindows7 = WindowsVersionHelper.IsSince(WindowsVersions.Win7) && !WindowsVersionHelper.IsSince(WindowsVersions.Win8);
		_configuration.ConnectionName = StringHelper.RemoveSpecialCharactersFromString(_configuration.ConnectionName);
		_wireGuardLogFile = Path.Combine(_configuration.ConfigDirectory, "log.bin");
		if (!Directory.Exists(Path.GetDirectoryName(_wireGuardLogFile)))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_wireGuardLogFile));
		}
		try
		{
			File.Delete(_wireGuardLogFile);
		}
		catch
		{
		}
		_ringLogger = new Ringlogger(_wireGuardLogFile, "GUI");
		_wireGuardLogsThread = new Thread(TailWireGuardLog)
		{
			IsBackground = true
		};
		_wireGuardLogsThread.Start();
		_logger?.LogInformation("Initialization completed.");
	}

	public void Dispose()
	{
		Disconnect();
		_wgInterfaceThreadStop.Dispose();
		_logsThreadStop.Set();
		_logsThreadStop.Dispose();
		try
		{
			_logger?.LogInformation("Waiting the WireGuard logs thread to complete");
			_wireGuardLogsThread.Join();
			_logger?.LogInformation("WireGuard logs thread complete");
		}
		catch (Exception ex)
		{
			_logger?.LogWarning("Exception occurred while waiting for the WireGuard logs thread to complete: ", ex);
		}
		_ringLogger.Dispose();
		DeleteWireGuardConfigFiles();
	}

	public async Task DisposeAsync()
	{
		await Task.Run(delegate
		{
			Dispose();
		}).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task Connect(Server server, IConnectionConfiguration connectionConfiguration, IUser user, SplitTunnelClientSettings splitTunnelClientSettings, DnsSettings dnsSettings, CancellationToken token = default(CancellationToken))
	{
		IWireGuardConnectionConfiguration wgConnectionConfiguration = connectionConfiguration as IWireGuardConnectionConfiguration;
		if (wgConnectionConfiguration == null)
		{
			throw new InvalidCastException("connectionConfiguration should be an instance of IWireGuardConnectionConfiguration");
		}
		_connectionHandled = new TaskCompletionSource<bool>();
		token.Register(delegate
		{
			_connectionHandled?.TrySetCanceled();
		});
		try
		{
			if (wgConnectionConfiguration.DoubleHopSettings != null && wgConnectionConfiguration.DoubleHopSettings.IsDoubleHopEnabled)
			{
				_wgConfig = await _apiManager.GenerateWireGuardConfiguration(token, server, wgConnectionConfiguration.DoubleHopSettings, wgConnectionConfiguration.AllowLan).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				_wgConfig = await _apiManager.GenerateWireGuardConfiguration(token, server, wgConnectionConfiguration.AllowLan).ConfigureAwait(continueOnCapturedContext: false);
			}
			if (dnsSettings.ShouldUpdateDns && (dnsSettings.DnsServers ?? Array.Empty<string>()).Length != 0)
			{
				_wgConfig.Interface.DNS = dnsSettings.DnsServers;
			}
			if (splitTunnelClientSettings.IsDomainBasedSplitTunnelEnabled)
			{
				_wgConfig.Interface.DNS = _wgConfig.Interface.DNS.Concat(new string[1] { IPAddress.Loopback.ToString() }).ToArray();
			}
			if (wgConnectionConfiguration.Mtu.HasValue)
			{
				_wgConfig.Interface.MTU = wgConnectionConfiguration.Mtu;
				_logger?.LogInformation($"WireGuard adapter MTU has been configured to {wgConnectionConfiguration.Mtu}.");
			}
			_configFile = ToggleAndPrepareConfigPath();
			Directory.CreateDirectory(Path.GetDirectoryName(_configFile));
			File.WriteAllText(_configFile, _wgConfig.ToWireGuardConfig());
			_logger?.LogInformation("WireGuard config created with path " + _configFile + ".");
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (HTTPException e) when (CanThrow(e))
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new WireGuardAPIException("There was an issue connecting to this location using WireGuard, please try another protocol or location.", inner);
		}
		token.ThrowIfCancellationRequested();
		try
		{
			_wgInterfaceThread = new Thread(TailAdapter)
			{
				IsBackground = true
			};
			_wgInterfaceThread.Start();
			await Task.Run(delegate
			{
				WireGuardService.Add(_configFile, ephemeral: true, wgConnectionConfiguration.ServiceStartTimeoutInSeconds);
			}, token).ConfigureAwait(continueOnCapturedContext: false);
			using CancellationTokenSource timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { token });
			timeoutToken.CancelAfter(TimeSpan.FromSeconds(30.0));
			timeoutToken.Token.Register(delegate
			{
				TaskCompletionSource<bool> connectionHandled = _connectionHandled;
				if (connectionHandled != null && !connectionHandled.Task.IsCompleted)
				{
					_connected = false;
					_connectionHandled.SetException(new TimeoutException("Connection could not complete."));
				}
			});
			if (_connectionHandled != null)
			{
				await _connectionHandled.Task.ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (OperationCanceledException)
		{
			await DisposeConnection().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
		catch (VpnHostServiceFileNotFoundException)
		{
			await DisposeConnection().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
		catch (Exception inner2)
		{
			await DisposeConnection().ConfigureAwait(continueOnCapturedContext: false);
			throw new VpnException("Unable to connect to the VPN server.", inner2);
		}
		_connected = true;
	}

	public async Task Disconnect()
	{
		if (_connected)
		{
			await DisposeConnection().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private bool CanThrow(Exception e)
	{
		if (!(e is OAuthException) && !(e is AuthenticationException) && !(e is InvalidAccountException) && !(e is InvalidServerException) && !(e is InvalidDoubleHopConfigurationException))
		{
			return e is DoubleHopNotAvailableException;
		}
		return true;
	}

	private string ToggleAndPrepareConfigPath()
	{
		_currentSuffix = (_currentSuffix + 1) % 2;
		string text = $"{_configuration.ConnectionName}_{_currentSuffix + 1}.conf";
		return Path.Combine(_configuration.ConfigDirectory, text ?? "");
	}

	private void DeleteRegistryEntries(string configName)
	{
		DeleteRegistryEntriesForPath(configName, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\NetworkList\\Signatures\\Unmanaged");
		DeleteRegistryEntriesForPath(configName, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\NetworkList\\Profiles");
	}

	private void DeleteRegistryEntriesForPath(string configName, string registryPath)
	{
		try
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryPath, writable: true);
			if (registryKey == null)
			{
				_logger?.LogWarning("Registry key not found: " + registryPath);
				return;
			}
			string[] subKeyNames = registryKey.GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				using RegistryKey registryKey2 = registryKey.OpenSubKey(text, writable: true);
				if (registryKey2 == null)
				{
					continue;
				}
				object? value = registryKey2.GetValue("Description");
				if (value != null && value.ToString().IndexOf(configName, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					try
					{
						registryKey.DeleteSubKeyTree(text);
						_logger?.LogDebug("Subkey '" + text + "' deleted successfully.");
					}
					catch (Exception ex)
					{
						_logger?.LogError("Error deleting sub key '" + text + "'", ex);
					}
				}
			}
		}
		catch (Exception ex2)
		{
			_logger?.LogWarning("Error accessing registry key '" + registryPath + "': " + ex2.Message);
		}
	}

	private void DeleteWireGuardConfigFiles()
	{
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_configuration.ConfigDirectory);
			if (!directoryInfo.Exists)
			{
				_logger?.LogInformation("The WireGuard config directory does not exist. No files to delete.");
				return;
			}
			FileInfo[] files = directoryInfo.GetFiles("*.conf");
			foreach (FileInfo fileInfo in files)
			{
				try
				{
					fileInfo.Delete();
					_logger?.LogDebug("Deleted WireGuard config file: " + fileInfo.Name);
				}
				catch (Exception ex)
				{
					_logger?.LogWarning("Failed to WireGuard config delete file " + fileInfo.Name + ". Error: " + ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			_logger?.LogError("An unexpected error occurred while deleting WireGuard config files", ex2);
		}
	}

	private void TailWireGuardLog()
	{
		uint cursor = Ringlogger.CursorAll;
		try
		{
			while (Thread.CurrentThread.IsAlive && !_logsThreadStop.Wait(1000))
			{
				foreach (string item in _ringLogger.FollowFromCursor(ref cursor))
				{
					_logger?.LogInformation(item);
				}
			}
		}
		catch (ObjectDisposedException arg)
		{
			_logger?.LogWarning($"Exception from ring logger: {arg}");
		}
	}

	private async Task DisposeConnection()
	{
		_logger?.LogDebug("Disposing WireGuard connection.");
		_wgInterfaceThreadStop.Set();
		try
		{
			if (_wgInterfaceThread != null)
			{
				_logger?.LogDebug("Awaiting interface thread to complete its activity.");
				_wgInterfaceThread.Join();
				_logger?.LogDebug("Interface thread complete.");
			}
		}
		catch (Exception ex)
		{
			_logger?.LogWarning("An error occurred during WireGuard log messages thread join.", ex);
		}
		_wgInterfaceThreadStop.Reset();
		await PurgeWireGuardConfigAsync().ConfigureAwait(continueOnCapturedContext: false);
		var (flag, serviceState) = WireGuardService.GetServiceStatus(_configFile);
		if (flag)
		{
			_logger?.LogWarning($"Service status check - Exists: {flag}, State: {serviceState}. Attempting to terminate VPN host service...");
			await SafelyTerminateVpnHostServiceAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		_logger?.LogDebug("WireGuard connection has been disposed.");
	}

	private async Task SafelyTerminateVpnHostServiceAsync(int timeoutMilliseconds = 5000)
	{
		try
		{
			Process[] processesByName = Process.GetProcessesByName("VpnHostService");
			Process[] array = processesByName;
			foreach (Process process in array)
			{
				try
				{
					_logger?.LogInformation($"Attempting to terminate VpnHostService (PID: {process.Id})");
					process.Kill();
					await WaitForExitAsync(process, timeoutMilliseconds).ConfigureAwait(continueOnCapturedContext: false);
					_logger?.LogInformation($"VpnHostService (PID: {process.Id}) terminated successfully");
				}
				catch (Exception ex)
				{
					_logger?.LogError("Error terminating VpnHostService: " + ex.Message);
				}
				finally
				{
					process.Dispose();
				}
			}
		}
		catch (Exception ex2)
		{
			_logger?.LogError("Error in SafelyTerminateVpnHostService: " + ex2.Message);
		}
	}

	private void PurgeWireGuardConfig()
	{
		try
		{
			_logger?.LogInformation("Removing WireGuard service.");
			WireGuardService.Remove(_configFile, waitForStop: true);
			_logger?.LogInformation("WireGuard service has been removed.");
		}
		catch (ObjectDisposedException arg)
		{
			_logger?.LogWarning($"WireGuard service has already been disposed {arg}");
		}
		DeleteRegistryEntries(Path.GetFileNameWithoutExtension(_configFile));
		_connected = false;
	}

	private async Task PurgeWireGuardConfigAsync()
	{
		try
		{
			await Task.Run((Action)PurgeWireGuardConfig).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception arg)
		{
			_logger?.LogWarning($"Exception when purging WireGuard config. {arg}");
		}
	}

	private Task<bool> WaitForExitAsync(Process process, int timeout)
	{
		return Task.Run(() => process.WaitForExit(timeout));
	}

	private void TailAdapter()
	{
		bool flag = false;
		Driver.Adapter adapter = null;
		while (Thread.CurrentThread.IsAlive && !_wgInterfaceThreadStop.Wait(1000) && adapter == null)
		{
			try
			{
				adapter = WireGuardService.GetAdapter(_configFile);
			}
			catch
			{
				continue;
			}
			break;
		}
		try
		{
			ulong num = 0uL;
			short num2 = 0;
			if (adapter == null)
			{
				_logger?.LogTrace("WireGuard adapter not found.");
				return;
			}
			while (Thread.CurrentThread.IsAlive && !_wgInterfaceThreadStop.Wait(1000))
			{
				ulong num3 = 0uL;
				Driver.Adapter.Peer[] peers = adapter.GetConfiguration().Peers;
				foreach (Driver.Adapter.Peer peer in peers)
				{
					num3 += peer.RxBytes;
				}
				if (num3 != 0 && _connectionHandled != null)
				{
					_connectionHandled.TrySetResult(result: true);
					_connectionHandled = null;
				}
				if (num == num3)
				{
					if (num2 > 5 && num2 % 2 == 0)
					{
						PingHelper.PingSendAsync(_wgConfig.Interface.DNS[0]);
					}
					if (num2 > 15 && _connected)
					{
						PurgeWireGuardConfig();
						UnexpectedDisconnect?.Invoke();
						break;
					}
					num2++;
				}
				else
				{
					num2 = 0;
				}
				num = num3;
				if (_isWindows7 && _connected && !WireGuardService.IsServiceRunning(_configFile))
				{
					_logger?.LogTrace("TailAdapter: Wireguard service stopped unexpectedly.");
					DisableNetworkAdapter();
					HandleUnexpectedDisconnect();
					break;
				}
			}
		}
		catch (Win32Exception ex) when (ex.ErrorCode == -2147467259)
		{
			flag = true;
			_logger?.LogError("TailAdapter:Adapter not found exception!!!", ex);
		}
		catch (Exception ex2)
		{
			_logger?.LogError("TailAdapter:Exception!!!", ex2);
		}
		finally
		{
			if (flag)
			{
				HandleUnexpectedDisconnect();
			}
		}
	}

	private void HandleUnexpectedDisconnect()
	{
		try
		{
			PurgeWireGuardConfig();
			UnexpectedDisconnect?.Invoke();
			IntPtr intPtr = NativeMethods.openAdapter(Path.GetFileNameWithoutExtension(_configFile));
			if (intPtr != IntPtr.Zero)
			{
				NativeMethods.freeAdapter(intPtr);
				_logger?.LogDebug("WireGuard adapter has been released");
			}
		}
		catch (Exception ex)
		{
			_logger?.LogError("Error while handling unexpected disconnect", ex);
		}
	}

	private void DisableNetworkAdapter()
	{
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE Name LIKE '%WireGuard%'"));
			foreach (ManagementObject item in managementObjectSearcher.Get())
			{
				_logger?.LogInformation(string.Format("DisableNetworkAdapter {0}", item["name"]));
				item.InvokeMethod("Disable", null);
			}
		}
		catch (Exception ex)
		{
			_logger?.LogError("Error when disabling WireGuard network adapter", ex);
		}
	}
}
