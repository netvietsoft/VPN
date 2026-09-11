using System;
using System.Collections.Generic;
using VpnSDK.Private.WFP.Filtering;
using VpnSDK.Private.WFP.Interop;
using VpnSDK.Private.WFP.Transaction;

namespace VpnSDK.Private.WFP;

internal class WFPEngine : WFPEngineBase, IWFPEngine, IDisposable
{
	public void StartWFPEngine(string engineName, bool runDynamically, bool enforceTransactionScope, ushort sublayerWeight)
	{
		FilterCollection = new FilterCollection(this);
		base.TransactionScopeEnforcement = enforceTransactionScope;
		InitializeEngine(engineName, runDynamically);
		ConfigureEngine(persistent: false, sublayerWeight);
	}

	private void ConfigureEngine(bool persistent, ushort sublayerWeight)
	{
		using (StartTransaction())
		{
			AddProvider(persistent);
			AddSubLayer(persistent, sublayerWeight);
		}
	}

	protected override List<ulong> Add(NetworkFilter filter)
	{
		WFPTransaction currentTransaction = CurrentTransaction;
		if (currentTransaction == null || currentTransaction.Completed)
		{
			throw new InvalidOperationException("Not in transaction block.");
		}
		List<ulong> list = new List<ulong>();
		if (filter.InEngine)
		{
			if (!filter.Enabled)
			{
				return list;
			}
		}
		else
		{
			filter._filter.providerKey = ProviderPtr;
			filter._filter.subLayerKey = SublayerState.subLayerKey;
		}
		filter.Conditions.UpdateUnmanagedConditionArray();
		foreach (GUID item in filter.AddressFamily switch
		{
			IPAddressFamily.IPv4 => LayerGuid.IPv4, 
			IPAddressFamily.IPv6 => LayerGuid.IPv6, 
			_ => LayerGuid.All, 
		})
		{
			filter._filter.layerKey = item;
			try
			{
				ulong id = 0uL;
				WFPNativeMethods.AddFilter(base.Handle, ref filter._filter, out id);
				list.Add(id);
			}
			catch (WFPException ex) when (ex.ErrorType == WFPError.FWP_E_FILTER_NOT_FOUND)
			{
			}
		}
		if (filter._associatedEngine == null)
		{
			filter._associatedEngine = this;
		}
		return list;
	}

	public void Dispose()
	{
		CleanUp(EngineSession.Name);
		base.DisposeEngine();
	}
}
