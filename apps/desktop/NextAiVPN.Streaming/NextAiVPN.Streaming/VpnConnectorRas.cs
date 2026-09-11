using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using DotRas;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Enums.Streaming;
using NextAiVPN.Streaming.Entities;

namespace NextAiVPN.Streaming;

public class VpnConnectorRas : IVpnConnector
{
	private readonly string _connectionName = StreamingConstants.VpnName;

	private readonly ISplitTunnelRoutingService _splitTunnelRoutingService;

	private readonly Protocol _protocol = Protocol.IKEV2;

	private string _userName;

	private string _password;

	private RasDialer _dialer;

	private RasDevice RasDevice
	{
		get
		{
			string name = Enum.GetName(typeof(Protocol), _protocol);
			return RasDevice.GetDevices().FirstOrDefault((RasDevice c) => c.Name.ToLower().Contains(name.ToLower())) ?? throw new Exception("No device found.");
		}
	}

	public bool IsActive => NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault((NetworkInterface ni) => ni.Description.Equals(_connectionName, StringComparison.OrdinalIgnoreCase) && ni.NetworkInterfaceType == NetworkInterfaceType.Ppp && ni.OperationalStatus == OperationalStatus.Up) != null;

	public VpnConnectorRas(ISplitTunnelRoutingService splitTunnelRoutingService)
	{
		_splitTunnelRoutingService = splitTunnelRoutingService;
	}

	public async Task<bool> TryConnect(IStreamingLocation streamingLocation)
	{
		if (IsActive)
		{
			return true;
		}
		try
		{
			foreach (IServer server in streamingLocation.Servers)
			{
				await CreateOrUpdateConnectionAsync(server.Name);
				Thread.Sleep(500);
				if (IsActive)
				{
					Logger.Log.Information("[Streaming] Connected. Location: " + streamingLocation.FullLocationName + ". Server: " + server.Name, "TryConnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 81);
					return IsActive;
				}
				Logger.Log.Error("Streaming connection error. Location: " + streamingLocation.FullLocationName + ". Server: " + server.Name, "TryConnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 87);
			}
		}
		catch (Exception)
		{
			Logger.Log.Error("Streaming connection error. Location: " + streamingLocation.FullLocationName, "TryConnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 93);
		}
		return false;
	}

	public bool TryDisconnect()
	{
		try
		{
			RasConnection rasConnection = RasConnection.GetActiveConnections().FirstOrDefault((RasConnection c) => c.EntryName == _connectionName);
			if (rasConnection != null)
			{
				rasConnection.HangUp();
				Logger.Log.Information("Disconnected successfully!", "TryDisconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 109);
			}
			else
			{
				Logger.Log.Information("No active connection found for the specified VPN.", "TryDisconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 113);
			}
			RemoveConnection();
		}
		catch (Exception ex)
		{
			Logger.Log.Error(ex.Message, "TryDisconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 120);
			return false;
		}
		return true;
	}

	public async Task<bool> TryDisconnectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		_ = 4;
		try
		{
			RasConnection activeConnection = await Task.Run(() => RasConnection.GetActiveConnections().FirstOrDefault((RasConnection c) => c.EntryName == _connectionName), cancellationToken);
			if (activeConnection != null)
			{
				await Task.Run(delegate
				{
					activeConnection.HangUp();
				}, cancellationToken);
				Logger.Log.Information("Disconnected successfully!", "TryDisconnectAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 140);
			}
			else
			{
				Logger.Log.Information("No active connection found for the specified VPN.", "TryDisconnectAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 144);
			}
			if (await _splitTunnelRoutingService.IsSplitTunnelingEnabledAsync())
			{
				await _splitTunnelRoutingService.RemoveAllBypassRoutesAsync();
			}
			await RemoveConnectionAsync(cancellationToken);
			return true;
		}
		catch (OperationCanceledException)
		{
			Logger.Log.Warning("Disconnect cancelled.", "TryDisconnectAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 157);
			return false;
		}
		catch (Exception ex2)
		{
			Logger.Log.Error("[Streaming] Error while disconnecting. " + ex2.Message, "TryDisconnectAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 162);
			return false;
		}
	}

	public async Task<bool> CreateOrUpdateConnectionAsync(string serverAddress)
	{
		try
		{
			RasPhoneBook allUsersPhoneBook = new RasPhoneBook();
			string phoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
			allUsersPhoneBook.Open(phoneBookPath);
			await RemoveConnectionAsync();
			RasEntry entry = RasEntry.CreateVpnEntry(_connectionName, serverAddress, RasVpnStrategy.IkeV2Only, RasDevice);
			entry.Options.UseLogOnCredentials = false;
			entry.Options.RemoteDefaultGateway = true;
			allUsersPhoneBook.Entries.Add(entry);
			if (await _splitTunnelRoutingService.IsSplitTunnelingEnabledAsync())
			{
				entry.Options.RemoteDefaultGateway = false;
				ExecuteProcess("Set-VpnConnection -Name \"" + _connectionName + "\" -SplitTunneling $true -AllUserConnection -Force");
			}
			ExecuteProcess("Set-VpnConnectionIPsecConfiguration -ConnectionName \"" + _connectionName + "\" -AuthenticationTransformConstants GCMAES256 -CipherTransformConstants GCMAES256 -EncryptionMethod GCMAES256 -IntegrityCheckMethod SHA384 -DHGroup ECP384 -PfsGroup ECP384 -Force");
			_dialer = new RasDialer
			{
				EntryName = _connectionName,
				PhoneBookPath = phoneBookPath,
				Credentials = new NetworkCredential(_userName, _password)
			};
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			_dialer.DialCompleted += delegate(object? sender, DialCompletedEventArgs e)
			{
				OnDialCompleted(sender, e, tcs);
			};
			_dialer.DialAsync();
			bool connected = await tcs.Task;
			if (connected && await _splitTunnelRoutingService.IsSplitTunnelingEnabledAsync())
			{
				await _splitTunnelRoutingService.RemoveBypassRoutesAsync();
				await _splitTunnelRoutingService.AddDomainsToSplitTunnel();
			}
			return connected;
		}
		catch (RasException ex)
		{
			Logger.Log.Error("[Streaming] RAS Exception: " + ex.Message, "CreateOrUpdateConnectionAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 235);
			return false;
		}
		catch (Exception ex2)
		{
			Logger.Log.Error("[Streaming] connecting Exception: " + ex2.Message, "CreateOrUpdateConnectionAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 240);
			return false;
		}
	}

	private void OnDialCompleted(object sender, DialCompletedEventArgs e, TaskCompletionSource<bool> tcs)
	{
		if (e.Error != null)
		{
			Logger.Log.Error("[Streaming] Dial failed: " + e.Error.Message, "OnDialCompleted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 251);
			tcs.TrySetResult(result: false);
		}
		else if (e.Connected)
		{
			Logger.Log.Information("[Streaming] Connected successfully!", "OnDialCompleted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 256);
			tcs.TrySetResult(result: true);
		}
		else
		{
			Logger.Log.Warning("[Streaming] Dial completed, but not connected.", "OnDialCompleted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 261);
			tcs.TrySetResult(result: false);
		}
	}

	public void CreateOrUpdateConnection(string serverAddress)
	{
		try
		{
			RasDialer rasDialer = new RasDialer();
			try
			{
				RasPhoneBook rasPhoneBook = new RasPhoneBook();
				try
				{
					string phoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
					rasPhoneBook.Open(phoneBookPath);
					if (rasPhoneBook.Entries.Contains(_connectionName))
					{
						rasPhoneBook.Entries[_connectionName].PhoneNumber = _connectionName;
						rasPhoneBook.Entries[_connectionName].VpnStrategy = RasVpnStrategy.IkeV2First;
						rasPhoneBook.Entries[_connectionName].Device = RasDevice;
						rasPhoneBook.Entries[_connectionName].Update();
					}
					else
					{
						RasEntry item = RasEntry.CreateVpnEntry(_connectionName, serverAddress, RasVpnStrategy.IkeV2Only, RasDevice);
						rasPhoneBook.Entries.Add(item);
						rasDialer.EntryName = _connectionName;
						rasDialer.PhoneBookPath = phoneBookPath;
					}
					rasDialer.Credentials = new NetworkCredential(_userName, _password);
					ExecuteProcess("Set-VpnConnectionIPsecConfiguration -ConnectionName \"" + _connectionName + "\" -AuthenticationTransformConstants GCMAES256 -CipherTransformConstants GCMAES256 -EncryptionMethod GCMAES256 -IntegrityCheckMethod SHA384 -DHGroup ECP384 -PfsGroup ECP384 -Force");
					rasDialer.DialCompleted += OnDialCompleted;
					rasDialer.Dial();
				}
				finally
				{
					((IDisposable)(object)rasPhoneBook)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)(object)rasDialer)?.Dispose();
			}
		}
		catch (RasException ex)
		{
			Logger.Log.Error("RAS Exception: " + ex.Message, "CreateOrUpdateConnection", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 307);
			Console.WriteLine();
		}
		catch (Exception ex2)
		{
			Logger.Log.Error("Streaming connecting Exception: " + ex2.Message, "CreateOrUpdateConnection", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 312);
		}
	}

	private void OnDialCompleted(object sender, DialCompletedEventArgs e)
	{
		if (e.Error != null)
		{
			Logger.Log.Error("Dial failed: " + e.Error.Message, "OnDialCompleted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 320);
		}
		else if (e.Connected)
		{
			Logger.Log.Information("Connected successfully!", "OnDialCompleted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 324);
		}
		else
		{
			Logger.Log.Warning("Dial completed, but not connected.", "OnDialCompleted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 328);
		}
	}

	public void RemoveConnection()
	{
		if (IsActive)
		{
			TryDisconnect();
		}
		string phoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
		RasPhoneBook rasPhoneBook = new RasPhoneBook();
		try
		{
			rasPhoneBook.Open(phoneBookPath);
			if (rasPhoneBook.Entries.Contains(_connectionName))
			{
				rasPhoneBook.Entries.Remove(_connectionName);
			}
		}
		catch (Exception ex)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = "rasdial.exe",
				Arguments = "\"" + _connectionName + "\" /disconnect",
				CreateNoWindow = true,
				UseShellExecute = false
			});
			Thread.Sleep(1000);
			if (rasPhoneBook.Entries.Contains(_connectionName))
			{
				rasPhoneBook.Entries.Remove(_connectionName);
			}
			Logger.Log.Error(ex.Message, "RemoveConnection", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 367);
		}
		finally
		{
			((IDisposable)(object)rasPhoneBook)?.Dispose();
		}
	}

	public async Task<bool> RemoveConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			if (IsActive)
			{
				await TryDisconnectAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			string phoneBookPath = await Task.Run(() => RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await Task.Run(async delegate
			{
				RasPhoneBook phoneBook = new RasPhoneBook();
				try
				{
					phoneBook.Open(phoneBookPath);
					if (!phoneBook.Entries.Contains(_connectionName))
					{
						return;
					}
					try
					{
						phoneBook.Entries.Remove(_connectionName);
					}
					catch (Exception ex3)
					{
						Logger.Log.Warning(ex3.Message, "RemoveConnectionAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 404);
						ProcessStartInfo startInfo = new ProcessStartInfo
						{
							FileName = "rasdial.exe",
							Arguments = "\"" + _connectionName + "\" /disconnect",
							CreateNoWindow = true,
							UseShellExecute = false
						};
						using (Process proc = Process.Start(startInfo))
						{
							if (proc != null)
							{
								await WaitForExitAsync(proc, cancellationToken);
							}
						}
						await Task.Delay(1000, cancellationToken);
						if (phoneBook.Entries.Contains(_connectionName))
						{
							phoneBook.Entries.Remove(_connectionName);
						}
					}
				}
				finally
				{
					((IDisposable)(object)phoneBook)?.Dispose();
				}
			}, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return true;
		}
		catch (OperationCanceledException)
		{
			Logger.Log.Warning("RemoveConnection was cancelled.", "RemoveConnectionAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 438);
			return false;
		}
		catch (Exception ex2)
		{
			Logger.Log.Error(ex2.Message, "RemoveConnectionAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\VpnConnectorRas.cs", 443);
			return false;
		}
	}

	private static Task WaitForExitAsync(Process process, CancellationToken token)
	{
		if (process.HasExited)
		{
			return Task.CompletedTask;
		}
		return Task.Run(delegate
		{
			using (token.Register(delegate
			{
				if (!process.HasExited)
				{
					try
					{
						process.Kill();
					}
					catch
					{
					}
				}
			}))
			{
				process.WaitForExit();
			}
		}, token);
	}

	public void SetCredentials(string userName, string password)
	{
		_userName = userName;
		_password = password;
	}

	private static void ExecuteProcess(string script)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = "powershell.exe",
			Arguments = "-Command \"" + script + "\"",
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};
		Process process = new Process();
		process.StartInfo = startInfo;
		process.Start();
		process.StandardOutput.ReadToEnd();
		process.WaitForExit();
	}
}
