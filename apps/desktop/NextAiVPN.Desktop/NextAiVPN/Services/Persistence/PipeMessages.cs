using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

public class PipeMessages
{
	private readonly string _pipeName;

	private const int DefaultTimeoutMs = 5000;

	private static readonly object _serverLock = new object();

	private static readonly HashSet<string> _startedServers = new HashSet<string>();

	private readonly IAppLogger _logger;

	public event EventHandler<MessageEventArgs> MessageReceived;

	public PipeMessages(string pipeName, IAppLogger logger)
	{
		_pipeName = pipeName;
		_logger = logger;
	}

	public async Task SendAsync(int msg, int timeoutMs = 5000, CancellationToken cancellationToken = default(CancellationToken))
	{
		_ = 1;
		try
		{
			using NamedPipeClientStream pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
			await pipe.ConnectAsync(timeoutMs, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			using BinaryWriter writer = new BinaryWriter(pipe);
			writer.Write(msg);
			_logger?.Information($"[{"SendAsync"}] Pipe name: '{_pipeName}'. Message sent: '{msg}'", "SendAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PipeMessages.cs", 59);
			await writer.FlushAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			_logger?.Error("[SendAsync] Operation canceled by token or timeout.", "SendAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PipeMessages.cs", 66);
			throw;
		}
		catch (Exception value)
		{
			_logger?.Error($"[{"SendAsync"}] Failed to send message: {value}", "SendAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PipeMessages.cs", 71);
			throw;
		}
	}

	public void StartServer(CancellationToken cancellationToken = default(CancellationToken))
	{
		lock (_serverLock)
		{
			if (_startedServers.Contains(_pipeName))
			{
				_logger?.Error("[StartServer] Server is already running. Ignoring duplicate start.", "StartServer", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PipeMessages.cs", 88);
				return;
			}
			_startedServers.Add(_pipeName);
		}
		Task.Run(async delegate
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				using NamedPipeServerStream server = new NamedPipeServerStream(_pipeName, PipeDirection.In, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				try
				{
					await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					using BinaryReader binaryReader = new BinaryReader(server);
					int num = binaryReader.ReadInt32();
					_logger?.Information($"[{"StartServer"}] Pipe name: '{_pipeName}'. Message received: '{num}'", "StartServer", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PipeMessages.cs", 112);
					MessageReceived?.Invoke(null, new MessageEventArgs(num));
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception value)
				{
					_logger?.Error($"[{"StartServer"}] Error in server loop: {value}", "StartServer", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\PipeMessages.cs", 122);
				}
			}
			lock (_serverLock)
			{
				_startedServers.Remove(_pipeName);
			}
		}, cancellationToken);
	}
}
