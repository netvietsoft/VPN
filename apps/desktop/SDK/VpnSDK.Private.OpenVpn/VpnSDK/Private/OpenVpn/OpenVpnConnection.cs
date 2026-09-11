using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VpnSDK.Private.OpenVpn.Configuration;
using VpnSDK.Private.OpenVpn.Management;
using VpnSDK.Private.OpenVpn.ProcessHandler;

namespace VpnSDK.Private.OpenVpn;

internal class OpenVpnConnection : IDisposable
{
	private readonly OpenVpnEnvironment _environment;

	private OpenVpnProcess _openVpnProcess;

	private Manager _manager;

	public bool IsDisposed { get; internal set; }

	public event Action UnexpectedDisconnect;

	public event Action<string> DataReceived;

	public OpenVpnConnection(OpenVpnEnvironment environment, NetworkCredential networkCredential, string privateKeyPassword = null)
	{
		_environment = environment;
		_openVpnProcess = new OpenVpnProcess(environment, networkCredential, privateKeyPassword);
	}

	public async Task Connect(OpenVpnConfiguration configuration, CancellationToken cancellationToken)
	{
		if (_openVpnProcess?.IsRunning ?? false)
		{
			throw new InvalidOperationException("OpenVPN process is already running.");
		}
		try
		{
			_manager = await _openVpnProcess.Start(configuration, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch
		{
			_manager?.Dispose();
			throw;
		}
		_manager.DataReceived += DataReceived;
		_manager.ConnectionClosed += OnConnectionClosed;
	}

	private void OnConnectionClosed()
	{
		Dispose();
		UnexpectedDisconnect?.Invoke();
	}

	public async Task Disconnect()
	{
		if (_manager != null)
		{
			_manager.DataReceived -= DataReceived;
			_manager.ConnectionClosed -= OnConnectionClosed;
			_manager.Dispose();
			_manager = null;
		}
		if (_openVpnProcess != null)
		{
			await _openVpnProcess.Stop().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			if (_manager != null)
			{
				_manager.DataReceived -= DataReceived;
				_manager.ConnectionClosed -= OnConnectionClosed;
				_manager.Dispose();
				_manager = null;
			}
			_openVpnProcess?.Dispose();
			_openVpnProcess = null;
			IsDisposed = true;
		}
	}
}
