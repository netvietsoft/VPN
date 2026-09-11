using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.WFP.Interop;
using VpnSDK.Private.WFP.Utilities;

namespace VpnSDK.Private.WFP.Transaction;

internal class WFPTransaction : IDisposable
{
	private readonly WFPEngineBase _wfpEngine;

	private readonly ILogger _logger;

	private readonly bool _ignoreFilterProcessException;

	public bool Completed;

	public WFPTransaction(WFPEngineBase engine, bool ignoreFilterProcessException = false)
	{
		if (engine == null)
		{
			throw new ArgumentNullException("engine", "Engine cannot be null.");
		}
		_wfpEngine = engine;
		_ignoreFilterProcessException = ignoreFilterProcessException;
		_logger = LogProvider.GetLogger("VpnSDK::WFPTransaction");
		Monitor.Enter(engine.TransactionLock);
		WFPNativeMethods.StartTransaction(_wfpEngine.Handle);
	}

	public void Dispose()
	{
		if (Completed)
		{
			if (Monitor.IsEntered(_wfpEngine.TransactionLock))
			{
				Monitor.Exit(_wfpEngine.TransactionLock);
			}
			_logger?.LogWarning("No filters deleted as transaction is complete.");
			return;
		}
		if (!_ignoreFilterProcessException && Marshal.GetExceptionCode() != 0)
		{
			_logger?.LogWarning($"Failed to remove filters due to a Win32 exception. Exception code: {Marshal.GetExceptionCode()}");
			WFPNativeMethods.FwpmTransactionAbort0(_wfpEngine.Handle);
			_wfpEngine.Filters.Undo();
			Completed = true;
			Monitor.Exit(_wfpEngine.TransactionLock);
			return;
		}
		try
		{
			_wfpEngine.ProcessTransaction();
			Completed = true;
		}
		catch
		{
			_wfpEngine.Filters.Undo();
			Completed = true;
			throw;
		}
		finally
		{
			Monitor.Exit(_wfpEngine.TransactionLock);
		}
	}
}
