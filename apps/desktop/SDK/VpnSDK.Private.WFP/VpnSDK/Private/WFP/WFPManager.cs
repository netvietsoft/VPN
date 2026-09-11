using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.WFP.Filtering;
using VpnSDK.Private.WFP.Utilities;

namespace VpnSDK.Private.WFP;

internal class WFPManager : IDisposable, IWFPManager
{
	private bool _isDisposed;

	private readonly IWFPEngine _engine;

	public FilterCollection Filters => _engine.Filters;

	public WFPManager(IWFPEngine engine, ILoggerFactory loggerFactory = null)
	{
		LogProvider.SetLogFactory(loggerFactory);
		_engine = engine;
	}

	public void StartGeneralEngine(string engineName, bool enforceTransactionScope = true)
	{
		if (string.IsNullOrEmpty(engineName))
		{
			engineName = GetEngineName();
		}
		_engine.StartWFPEngine(engineName, runDynamically: true, enforceTransactionScope, 2047);
	}

	public void StopGeneralEngine()
	{
		_engine.Dispose();
	}

	public IDisposable StartTransaction(bool ignoreFilterProcessException = false)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("WFPManager");
		}
		return _engine.StartTransaction(ignoreFilterProcessException);
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			StopGeneralEngine();
			_isDisposed = true;
		}
	}

	private string GetEngineName()
	{
		try
		{
			return Assembly.GetEntryAssembly().GetName().Name;
		}
		catch
		{
			return "TestEngine" + DateTime.Now.Ticks;
		}
	}

	public void CleanUpExistingFilters(string engineName)
	{
		_engine?.CleanUp(engineName.Split('.')[0]);
	}
}
