#define TRACE
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TentacleSoftware.Telnet;

internal class TelnetClient : IDisposable
{
	private readonly int _port;

	private readonly string _host;

	private readonly TimeSpan _sendRate;

	private readonly SemaphoreSlim _sendRateLimit;

	private readonly CancellationTokenSource _internalCancellation;

	private TcpClient _tcpClient;

	private StreamReader _tcpReader;

	private StreamWriter _tcpWriter;

	public EventHandler<string> MessageReceived;

	public EventHandler ConnectionClosed;

	private bool _disposed;

	public TelnetClient(string host, int port, TimeSpan sendRate, CancellationToken token)
	{
		_host = host;
		_port = port;
		_sendRate = sendRate;
		_sendRateLimit = new SemaphoreSlim(1);
		_internalCancellation = new CancellationTokenSource();
		token.Register(delegate
		{
			_internalCancellation.Cancel();
		});
	}

	public async Task Connect()
	{
		if (_tcpClient != null)
		{
			throw new NotSupportedException("Connect aborted: Reconnecting is not supported. You must dispose of this instance and instantiate a new TelnetClient");
		}
		_tcpClient = new TcpClient();
		await _tcpClient.ConnectAsync(_host, _port);
		_tcpReader = new StreamReader(_tcpClient.GetStream());
		_tcpWriter = new StreamWriter(_tcpClient.GetStream())
		{
			AutoFlush = true
		};
		WaitForMessage();
	}

	public async Task Connect(string socks4ProxyHost, int socks4ProxyPort, string socks4ProxyUser)
	{
		if (_tcpClient != null)
		{
			throw new NotSupportedException("Connect aborted: Reconnecting is not supported. You must dispose of this instance and instantiate a new TelnetClient");
		}
		_tcpClient = new TcpClient();
		await _tcpClient.ConnectAsync(socks4ProxyHost, socks4ProxyPort);
		byte[] addressBytes = Dns.GetHostAddresses(_host).First().GetAddressBytes();
		byte[] obj = new byte[2]
		{
			Convert.ToByte(_port / 256),
			Convert.ToByte(_port % 256)
		};
		byte[] bytes = Encoding.ASCII.GetBytes(socks4ProxyUser ?? string.Empty);
		byte[] array = new byte[9 + bytes.Length];
		array[0] = 4;
		array[1] = 1;
		obj.CopyTo(array, 2);
		addressBytes.CopyTo(array, 4);
		bytes.CopyTo(array, 8);
		array[8 + bytes.Length] = 0;
		await _tcpClient.GetStream().WriteAsync(array, 0, array.Length, _internalCancellation.Token);
		byte[] proxyResponse = new byte[8];
		await _tcpClient.GetStream().ReadAsync(proxyResponse, 0, proxyResponse.Length, _internalCancellation.Token);
		if (proxyResponse[1] != 90)
		{
			switch (proxyResponse[1])
			{
			case 91:
				throw new InvalidOperationException("Proxy connect request rejected or failed");
			case 92:
				throw new InvalidOperationException("Proxy connect request failed because client is not running identd (or not reachable from the server)");
			case 93:
				throw new InvalidOperationException("Proxy connect request failed because client's identd could not confirm the user ID string in the request");
			default:
				throw new InvalidOperationException("Proxy connect request failed, unknown error occured");
			}
		}
		_tcpReader = new StreamReader(_tcpClient.GetStream());
		_tcpWriter = new StreamWriter(_tcpClient.GetStream())
		{
			AutoFlush = true
		};
		WaitForMessage();
	}

	public async Task Send(string message)
	{
		_ = 2;
		try
		{
			await _sendRateLimit.WaitAsync(_internalCancellation.Token);
			await _tcpWriter.WriteLineAsync(message);
			await Task.Delay(_sendRate, _internalCancellation.Token);
		}
		catch (OperationCanceledException)
		{
			Trace.TraceInformation("Send aborted: IsCancellationRequested == true");
		}
		catch (ObjectDisposedException)
		{
			Trace.TraceInformation("Send failed: _tcpWriter or BaseStream disposed");
		}
		catch (IOException)
		{
			Trace.TraceError("Send failed: Socket disconnected unexpectedly");
			throw;
		}
		catch (Exception arg)
		{
			Trace.TraceError(string.Format("{0} failed: {1}", "Send", arg));
			throw;
		}
		finally
		{
			_sendRateLimit.Release();
		}
	}

	private async Task WaitForMessage()
	{
		try
		{
			while (!_internalCancellation.IsCancellationRequested)
			{
				string text;
				try
				{
					if (!_tcpClient.Connected)
					{
						Trace.TraceInformation("WaitForMessage aborted: _tcpClient is not connected");
						return;
					}
					text = await _tcpReader.ReadLineAsync();
					if (text == null)
					{
						Trace.TraceInformation("WaitForMessage aborted: _tcpReader reached end of stream");
						return;
					}
				}
				catch (ObjectDisposedException)
				{
					Trace.TraceInformation("WaitForMessage aborted: _tcpReader or BaseStream disposed. This is expected after calling Disconnect()");
					return;
				}
				catch (IOException)
				{
					Trace.TraceError("WaitForMessage aborted: Socket disconnected unexpectedly");
					return;
				}
				catch (Exception arg)
				{
					Trace.TraceError(string.Format("{0} aborted: {1}", "WaitForMessage", arg));
					return;
				}
				Trace.TraceInformation(string.Format("{0} received: {1} [{2}]", "WaitForMessage", text, text.Length));
				OnMessageReceived(text);
			}
			Trace.TraceInformation("WaitForMessage aborted: IsCancellationRequested == true");
		}
		finally
		{
			Trace.TraceInformation("WaitForMessage completed: Calling Disconnect");
			Disconnect();
		}
	}

	public void Disconnect()
	{
		try
		{
			_internalCancellation.Cancel();
			_tcpReader?.Close();
			_tcpWriter?.Close();
			_tcpClient?.Close();
		}
		catch (Exception arg)
		{
			Trace.TraceError(string.Format("{0} error: {1}", "Disconnect", arg));
		}
		finally
		{
			OnConnectionClosed();
		}
	}

	private void OnMessageReceived(string message)
	{
		MessageReceived?.Invoke(this, message);
	}

	private void OnConnectionClosed()
	{
		ConnectionClosed?.Invoke(this, new EventArgs());
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				Disconnect();
			}
			_disposed = true;
		}
	}
}
