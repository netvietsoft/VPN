using System;

namespace VpnSDK.Internal.Helpers;

internal class Disposable : IDisposable
{
	private readonly Action _action;

	private bool _disposed;

	private Disposable(Action action)
	{
		_action = action;
	}

	public static IDisposable Create(Action action)
	{
		return new Disposable(action);
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_action?.Invoke();
			_disposed = true;
		}
	}
}
