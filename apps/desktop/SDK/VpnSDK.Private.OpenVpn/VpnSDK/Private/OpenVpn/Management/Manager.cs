using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TentacleSoftware.Telnet;
using VpnSDK.Private.OpenVpn.Enums;
using VpnSDK.Private.OpenVpn.Exceptions;
using VpnSDK.Private.OpenVpn.Helpers;
using VpnSDK.Private.OpenVpn.ProcessHandler.Output;
using VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

namespace VpnSDK.Private.OpenVpn.Management;

internal class Manager : IDisposable
{
	private Process _process;

	private TelnetClient _telnetClient;

	private readonly string _miPassword;

	private Exception _fatalException;

	private ConnectionState _vpnState;

	private OutputParser _parser;

	private CancellationToken _cancellationToken;

	private readonly ILogger _logger = LogProvider.GetLogger("VpnSDK::Private::OpenVPN");

	internal bool IsEstablished { get; set; }

	internal NetworkCredential VpnCredential { get; set; }

	internal string PrivateKeyPassword { get; set; }

	internal event Action<string> DataReceived;

	internal event Action ConnectionClosed;

	internal Manager(Process process, string host, int port, CancellationToken cancellationToken, string miPassword = null)
	{
		_process = process;
		_cancellationToken = cancellationToken;
		_telnetClient = new TelnetClient(host, port, TimeSpan.FromMilliseconds(500.0), cancellationToken);
		TelnetClient telnetClient = _telnetClient;
		telnetClient.MessageReceived = (EventHandler<string>)Delegate.Combine(telnetClient.MessageReceived, new EventHandler<string>(OnMessageReceived));
		TelnetClient telnetClient2 = _telnetClient;
		telnetClient2.ConnectionClosed = (EventHandler)Delegate.Combine(telnetClient2.ConnectionClosed, new EventHandler(OnConnectionClosed));
		_miPassword = miPassword;
		_parser = new OutputParser();
	}

	private async void OnMessageReceived(object sender, string e)
	{
		if (e.Length == 0)
		{
			return;
		}
		IOutputMessage outputMessage = _parser.Parse(e);
		try
		{
			if (outputMessage is HoldMessage)
			{
				await _telnetClient.Send("hold release");
			}
			else if (outputMessage is PasswordMessage passwordMessage)
			{
				if (passwordMessage.State == AuthenticationState.NEED_USERNAME_PASSWORD)
				{
					await _telnetClient.Send("username 'Auth' " + VpnCredential.UserName);
					await _telnetClient.Send("password 'Auth' '" + VpnCredential.Password + "'");
				}
				else if (passwordMessage.State == AuthenticationState.NEED_PKEY_PASSWORD)
				{
					await _telnetClient.Send("password 'Private Key' " + PrivateKeyPassword);
				}
			}
			else if (outputMessage is FatalMessage fatalMessage)
			{
				_fatalException = fatalMessage.Exception;
				_logger?.LogCritical(_fatalException, e);
				TerminateManagementInterface();
			}
			else if (outputMessage is StateMessage stateMessage)
			{
				_vpnState = stateMessage.State;
			}
		}
		catch (IOException ex)
		{
			_logger?.LogError("Send failed: Socket disconnected unexpectedly ({0})", ex.Message);
			TerminateProcess();
		}
		catch (Exception ex2)
		{
			_logger?.LogError("Unknown error ({0})", ex2.Message);
			TerminateProcess();
		}
		DataReceived?.Invoke(e);
	}

	private void OnConnectionClosed(object sender, EventArgs e)
	{
		IsEstablished = false;
		if (_fatalException == null && _vpnState != ConnectionState.CONNECTED)
		{
			_fatalException = new ConnectionClosedException("Management Interface connection was closed.");
			_logger?.LogCritical(_fatalException, "Management Interface connection was closed.");
		}
		ConnectionClosed?.Invoke();
	}

	internal async Task EstablishManagementInterface()
	{
		await _telnetClient.Connect().ConfigureAwait(continueOnCapturedContext: false);
		_logger?.LogTrace("Management Interface was established.");
		if (_miPassword != null)
		{
			await _telnetClient.Send(_miPassword).ConfigureAwait(continueOnCapturedContext: false);
		}
		await _telnetClient.Send("log on").ConfigureAwait(continueOnCapturedContext: false);
		await _telnetClient.Send("state on").ConfigureAwait(continueOnCapturedContext: false);
		IsEstablished = true;
	}

	internal async Task WaitForVpnConnection(TimeSpan timeout)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while (!_cancellationToken.IsCancellationRequested)
		{
			if ((double)sw.ElapsedMilliseconds > timeout.TotalMilliseconds)
			{
				sw.Stop();
				throw new VpnSDK.Private.OpenVpn.Exceptions.TimeoutException("Connection timed out.");
			}
			if (_fatalException != null)
			{
				sw.Stop();
				throw _fatalException;
			}
			if (_vpnState == ConnectionState.CONNECTED)
			{
				sw.Stop();
				return;
			}
			await Task.Delay(100).ConfigureAwait(continueOnCapturedContext: false);
		}
		sw.Stop();
		if (_cancellationToken.IsCancellationRequested)
		{
			_cancellationToken.ThrowIfCancellationRequested();
		}
	}

	internal void TerminateManagementInterface()
	{
		if (IsEstablished)
		{
			_telnetClient.Disconnect();
			_logger?.LogTrace("Management Interface was terminated.");
		}
		IsEstablished = false;
	}

	private void TerminateProcess()
	{
		TerminateManagementInterface();
		try
		{
			_process?.Kill();
		}
		catch
		{
		}
	}

	internal void SendMessage(string message)
	{
		if (!IsEstablished)
		{
			throw new InvalidOperationException("Manager is not connected to OpenVPN Management Interface");
		}
		_telnetClient.Send(message);
	}

	public void Dispose()
	{
		if (IsEstablished)
		{
			_logger?.LogTrace("Disposing MI manager.");
			TerminateManagementInterface();
		}
		TelnetClient telnetClient = _telnetClient;
		telnetClient.MessageReceived = (EventHandler<string>)Delegate.Remove(telnetClient.MessageReceived, new EventHandler<string>(OnMessageReceived));
		TelnetClient telnetClient2 = _telnetClient;
		telnetClient2.ConnectionClosed = (EventHandler)Delegate.Remove(telnetClient2.ConnectionClosed, new EventHandler(OnConnectionClosed));
		_telnetClient.Dispose();
	}
}
