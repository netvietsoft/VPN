using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.OpenVpn.Configuration;
using VpnSDK.Private.OpenVpn.Helpers;
using VpnSDK.Private.OpenVpn.Management;

namespace VpnSDK.Private.OpenVpn.ProcessHandler;

internal class OpenVpnProcess : IDisposable
{
	private readonly ProcessStartInfo _processStartInfo = new ProcessStartInfo();

	private Process _process;

	private volatile bool _isRunning;

	private OpenVpnEnvironment _environment;

	private readonly NetworkCredential _networkCredential;

	private readonly string _privateKeyPassword;

	private readonly ILogger _logger = LogProvider.GetLogger("VpnSDK::Private::OpenVPN");

	private readonly ILogger _stderrLogger = LogProvider.GetLogger("VpnSDK::Private::OpenVPN::Process::StdErr");

	private readonly ILogger _stdoutLogger = LogProvider.GetLogger("VpnSDK::Private::OpenVPN::Process::StdOut");

	internal bool IsRunning => _isRunning;

	internal event EventHandler ProcessExited;

	internal OpenVpnProcess(OpenVpnEnvironment environment, NetworkCredential networkCredential, string privateKeyPassword = null)
	{
		_environment = environment;
		_processStartInfo.FileName = Path.Combine(_environment.ApplicationDirectory, _environment.ExecutableName);
		_processStartInfo.WorkingDirectory = environment.ApplicationDirectory;
		_processStartInfo.UseShellExecute = false;
		_processStartInfo.CreateNoWindow = true;
		_processStartInfo.RedirectStandardInput = true;
		_processStartInfo.RedirectStandardError = true;
		_processStartInfo.RedirectStandardOutput = true;
		_networkCredential = networkCredential;
		_privateKeyPassword = privateKeyPassword;
	}

	internal async Task<Manager> Start(OpenVpnConfiguration configuration, CancellationToken cancellationToken)
	{
		string miHost = "127.0.0.1";
		int miPort = TcpHelper.FindPort();
		string miPassword = Guid.NewGuid().ToString("N").ToString();
		_processStartInfo.Arguments = configuration.ToArguments() + $"--management {miHost} {miPort} stdin " + "--management-query-passwords --management-hold --management-signal --management-forget-disconnect";
		_process = new Process
		{
			StartInfo = _processStartInfo
		};
		_process.ErrorDataReceived += OnProcessStdErrOutput;
		_process.OutputDataReceived += OnProcessStdOutOutput;
		_process.Exited += OnProcessExit;
		_process.EnableRaisingEvents = true;
		_process.Start();
		_process.BeginOutputReadLine();
		_process.BeginErrorReadLine();
		_isRunning = true;
		await _process.StandardInput.WriteLineAsync(miPassword).ConfigureAwait(continueOnCapturedContext: false);
		if (cancellationToken.IsCancellationRequested)
		{
			await Stop().ConfigureAwait(continueOnCapturedContext: false);
			cancellationToken.ThrowIfCancellationRequested();
		}
		Manager manager = null;
		try
		{
			manager = new Manager(_process, miHost, miPort, cancellationToken, miPassword);
			if (_networkCredential != null)
			{
				manager.VpnCredential = _networkCredential;
			}
			if (!string.IsNullOrEmpty(_privateKeyPassword))
			{
				manager.PrivateKeyPassword = _privateKeyPassword;
			}
			await manager.EstablishManagementInterface().ConfigureAwait(continueOnCapturedContext: false);
			await manager.WaitForVpnConnection(TimeSpan.FromSeconds(90.0)).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch
		{
			manager?.Dispose();
			await Stop().ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
		return manager;
	}

	private void OnProcessStdOutOutput(object sender, DataReceivedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.Data))
		{
			_stdoutLogger?.LogTrace("StdOutOutput:" + e.Data);
		}
	}

	private void OnProcessStdErrOutput(object sender, DataReceivedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.Data))
		{
			_stderrLogger?.LogTrace("StdErrOutput:" + e.Data);
		}
	}

	private void OnProcessExit(object sender, EventArgs e)
	{
		if (sender is Process process)
		{
			if (process.HasExited)
			{
				try
				{
					_logger?.LogTrace("Exit code: {ExitCode}", process.ExitCode);
				}
				catch
				{
				}
			}
			else
			{
				try
				{
					process?.Kill();
				}
				catch
				{
				}
			}
		}
		_isRunning = false;
		_process = null;
		ProcessExited?.Invoke(sender, e);
	}

	public async Task Stop()
	{
		if (!_isRunning)
		{
			return;
		}
		try
		{
			if (_process == null)
			{
				return;
			}
			_process.OutputDataReceived -= OnProcessStdOutOutput;
			_process.ErrorDataReceived -= OnProcessStdErrOutput;
			_process.Exited -= OnProcessExit;
			_process.Refresh();
			if (ProcessIsRunning(_process))
			{
				try
				{
					_process.StandardInput.Write('\u001a');
					_process.StandardInput.Close();
				}
				catch
				{
				}
				_process.Kill();
				using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3.0));
				await ProcessExtensions.WaitForExitAsync(_process, cancellationTokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Could not kill OpenVPN process.");
		}
		finally
		{
			_isRunning = false;
		}
	}

	public void Dispose()
	{
		if (!_isRunning)
		{
			return;
		}
		try
		{
			_process?.Kill();
			_process = null;
		}
		catch
		{
		}
		finally
		{
			_isRunning = false;
		}
	}

	private static bool ProcessIsRunning(Process p)
	{
		try
		{
			return !p.HasExited && p.Threads.Count > 0;
		}
		catch
		{
			return false;
		}
	}
}
