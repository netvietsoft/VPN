using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.WFP.Filtering;
using VpnSDK.Private.WFP.Interop;
using VpnSDK.Private.WFP.Transaction;
using VpnSDK.Private.WFP.Utilities;

namespace VpnSDK.Private.WFP;

internal abstract class WFPEngineBase
{
	internal WFPTransaction CurrentTransaction;

	internal FilterCollection FilterCollection;

	internal IntPtr ProviderPtr;

	internal object TransactionLock = new object();

	private ILogger _logger;

	protected FWPM_SESSION0 EngineSession;

	protected FWPM_PROVIDER0 PolicyProvider;

	protected FWPM_SUBLAYER0 SublayerState;

	private FWPM_FILTER_ENUM_TEMPLATE0 enumTemplate;

	public FilterCollection Filters => FilterCollection;

	public bool InTransaction
	{
		get
		{
			if (CurrentTransaction != null)
			{
				return !CurrentTransaction.Completed;
			}
			return false;
		}
	}

	internal bool TransactionScopeEnforcement { get; set; }

	internal SafeWFPEngineHandle Handle { get; private set; }

	protected abstract List<ulong> Add(NetworkFilter filter);

	protected void InitializeEngine(string engineName, bool runDynamically)
	{
		EngineSession.Name = engineName;
		EngineSession.Flags = (runDynamically ? 1u : 0u);
		Handle = SafeWFPEngineHandle.CreateEngine(ref EngineSession);
	}

	protected void AddSubLayer(bool persistent, ushort sublayerWeight)
	{
		ProviderPtr = Marshal.AllocHGlobal(Marshal.SizeOf<GUID>());
		Marshal.StructureToPtr(PolicyProvider.providerKey, ProviderPtr, fDeleteOld: true);
		SublayerState.Name = EngineSession.Name + "-Sublayer";
		SublayerState.weight = sublayerWeight;
		SublayerState.subLayerKey = DeterministicGuid.Create(DeterministicGuid.DnsNamespace, "sublayer." + EngineSession.Name + ".wfp");
		SublayerState.providerKey = ProviderPtr;
		SublayerState.Flags = 0u;
		if (persistent)
		{
			SublayerState.Flags |= 1u;
		}
		WFPNativeMethods.AddSublayer(Handle, ref SublayerState);
	}

	protected void AddProvider(bool persistent)
	{
		PolicyProvider.Name = EngineSession.Name + "-Provider";
		PolicyProvider.providerKey = DeterministicGuid.Create(DeterministicGuid.DnsNamespace, "provider." + EngineSession.Name + ".wfp");
		PolicyProvider.Flags = 0u;
		if (persistent)
		{
			PolicyProvider.Flags |= 1u;
		}
		WFPNativeMethods.AddProvider(Handle, ref PolicyProvider);
	}

	public IDisposable StartTransaction(bool ignoreFilterProcessException = false)
	{
		WFPTransaction currentTransaction = CurrentTransaction;
		if (currentTransaction == null || currentTransaction.Completed)
		{
			CurrentTransaction = null;
		}
		CurrentTransaction = new WFPTransaction(this, ignoreFilterProcessException);
		return CurrentTransaction;
	}

	internal void ProcessTransaction()
	{
		foreach (NetworkFilter item in FilterCollection.Where((NetworkFilter x) => x.IsChanged && !FilterCollection.Transactions.Select((TransactionOperation y) => y.Filter).Contains(x)))
		{
			FilterCollection.Transactions.Add(new TransactionOperation(TransactionOperationType.Update, item));
		}
		try
		{
			foreach (TransactionOperation operation in FilterCollection.Transactions)
			{
				if ((operation.Operation == TransactionOperationType.Remove || operation.Operation == TransactionOperationType.Update) && !FilterCollection.Transactions.Any((TransactionOperation x) => x.Operation != TransactionOperationType.Add && x.Filter == operation.Filter && x.HasExecuted))
				{
					Remove(operation.Filter);
				}
				if (operation.Operation == TransactionOperationType.Add || operation.Operation == TransactionOperationType.Update)
				{
					operation.NewID.AddRange(Add(operation.Filter));
				}
				operation.HasExecuted = true;
			}
			WFPNativeMethods.EndTransaction(Handle);
			FilterCollection.Transactions.ForEach(delegate(TransactionOperation x)
			{
				x.Flush();
			});
		}
		catch
		{
			WFPNativeMethods.FwpmTransactionAbort0(Handle);
			throw;
		}
		finally
		{
			FilterCollection.Transactions.Clear();
		}
	}

	protected virtual void DisposeEngine()
	{
		if (Handle != null)
		{
			WFPTransaction currentTransaction = CurrentTransaction;
			if (currentTransaction != null && !currentTransaction.Completed)
			{
				WFPNativeMethods.FwpmTransactionAbort0(Handle);
			}
			Handle.Dispose();
			Handle.Close();
			FilterCollection?.Dispose();
			FilterCollection = null;
			CurrentTransaction = null;
			Handle = null;
		}
		if (ProviderPtr != IntPtr.Zero)
		{
			Marshal.DestroyStructure<GUID>(ProviderPtr);
			Marshal.FreeHGlobal(ProviderPtr);
			ProviderPtr = IntPtr.Zero;
		}
	}

	private void Remove(NetworkFilter filter)
	{
		_logger = LogProvider.GetLogger("VpnSDK::WFP");
		WFPTransaction currentTransaction = CurrentTransaction;
		if (currentTransaction == null || currentTransaction.Completed)
		{
			throw new InvalidOperationException("Not in transaction block.");
		}
		if (!filter.InEngine)
		{
			return;
		}
		foreach (ulong filterId in filter.FilterIds)
		{
			try
			{
				WFPNativeMethods.RemoveFilter(Handle, filterId);
			}
			catch (WFPException ex) when (ex.ErrorType == WFPError.FWP_E_FILTER_NOT_FOUND)
			{
				_logger?.LogWarning($"Filter with id {filterId} not found when removing.");
			}
		}
		filter._associatedEngine = null;
	}

	private void RemoveAllFilters(ref IntPtr providerKey)
	{
		if (Handle == null)
		{
			return;
		}
		enumTemplate.Clear();
		enumTemplate.providerKey = providerKey;
		enumTemplate.flags = 24u;
		enumTemplate.actionMask = uint.MaxValue;
		foreach (GUID item in LayerGuid.All)
		{
			enumTemplate.layerKey = item;
			SafeWFPFilterEnumHandle safeWFPFilterEnumHandle = SafeWFPFilterEnumHandle.CreateHandle(Handle, ref enumTemplate);
			if (safeWFPFilterEnumHandle == null)
			{
				continue;
			}
			IntPtr entries = IntPtr.Zero;
			try
			{
				uint numEntriesReturned = 0u;
				if (WFPNativeMethods.FwpmFilterEnum0(Handle, safeWFPFilterEnumHandle, uint.MaxValue, out entries, out numEntriesReturned) == 0 && entries != IntPtr.Zero && numEntriesReturned != 0)
				{
					for (int i = 0; i < numEntriesReturned; i++)
					{
						FWPM_FILTER0 fWPM_FILTER = Marshal.PtrToStructure<FWPM_FILTER0>(Marshal.ReadIntPtr(IntPtr.Add(entries, i * IntPtr.Size)));
						WFPNativeMethods.FwpmFilterDeleteById0(Handle, fWPM_FILTER.filterId);
					}
				}
			}
			finally
			{
				if (entries != IntPtr.Zero)
				{
					WFPNativeMethods.FwpmFreeMemory0(ref entries);
					entries = IntPtr.Zero;
				}
				safeWFPFilterEnumHandle.Dispose();
			}
		}
	}

	private void CleanFiltersAndProviders(string engineName, bool filterOnly)
	{
		if (Handle == null)
		{
			return;
		}
		SafeWFPProviderEnumHandle safeWFPProviderEnumHandle = SafeWFPProviderEnumHandle.CreateHandle(Handle);
		if (safeWFPProviderEnumHandle == null)
		{
			return;
		}
		IntPtr entries = IntPtr.Zero;
		try
		{
			uint numEntriesReturned = 0u;
			if (WFPNativeMethods.FwpmProviderEnum0(Handle, safeWFPProviderEnumHandle, uint.MaxValue, out entries, out numEntriesReturned) != 0 || !(entries != IntPtr.Zero) || numEntriesReturned == 0)
			{
				return;
			}
			for (int i = 0; i < numEntriesReturned; i++)
			{
				FWPM_PROVIDER0 fWPM_PROVIDER = Marshal.PtrToStructure<FWPM_PROVIDER0>(Marshal.ReadIntPtr(IntPtr.Add(entries, i * IntPtr.Size)));
				if (fWPM_PROVIDER.displayData.Name.Contains(engineName))
				{
					if (filterOnly)
					{
						IntPtr providerKey = Marshal.AllocHGlobal(Marshal.SizeOf<GUID>());
						Marshal.StructureToPtr(fWPM_PROVIDER.providerKey, providerKey, fDeleteOld: false);
						RemoveAllFilters(ref providerKey);
						Marshal.FreeHGlobal(providerKey);
					}
					else
					{
						WFPNativeMethods.FwpmProviderDeleteByKey0(Handle, ref fWPM_PROVIDER.providerKey);
					}
				}
			}
		}
		finally
		{
			if (entries != IntPtr.Zero)
			{
				WFPNativeMethods.FwpmFreeMemory0(ref entries);
				entries = IntPtr.Zero;
			}
			safeWFPProviderEnumHandle.Dispose();
		}
	}

	private void CleanSublayers(string enginePrefix)
	{
		if (Handle == null)
		{
			return;
		}
		SafeWFPSublayerEnumHandle safeWFPSublayerEnumHandle = SafeWFPSublayerEnumHandle.CreateHandle(Handle);
		if (safeWFPSublayerEnumHandle == null)
		{
			return;
		}
		IntPtr entries = IntPtr.Zero;
		try
		{
			uint numEntriesReturned = 0u;
			if (WFPNativeMethods.FwpmSubLayerEnum0(Handle, safeWFPSublayerEnumHandle, uint.MaxValue, out entries, out numEntriesReturned) != 0 || !(entries != IntPtr.Zero) || numEntriesReturned == 0)
			{
				return;
			}
			for (int i = 0; i < numEntriesReturned; i++)
			{
				FWPM_SUBLAYER0 fWPM_SUBLAYER = Marshal.PtrToStructure<FWPM_SUBLAYER0>(Marshal.ReadIntPtr(IntPtr.Add(entries, i * IntPtr.Size)));
				if (fWPM_SUBLAYER.displayData.Name.Contains(enginePrefix))
				{
					WFPNativeMethods.FwpmSubLayerDeleteByKey0(Handle, ref fWPM_SUBLAYER.subLayerKey);
				}
			}
		}
		finally
		{
			if (entries != IntPtr.Zero)
			{
				WFPNativeMethods.FwpmFreeMemory0(ref entries);
				entries = IntPtr.Zero;
			}
			safeWFPSublayerEnumHandle.Dispose();
		}
	}

	public virtual void CleanUp(string engineName)
	{
		bool flag = Handle == null;
		if (flag)
		{
			string engineName2 = engineName + "-CleanUpSession";
			InitializeEngine(engineName2, runDynamically: false);
		}
		CleanFiltersAndProviders(engineName, filterOnly: true);
		CleanSublayers(engineName);
		CleanFiltersAndProviders(engineName, filterOnly: false);
		if ((Handle != null) & flag)
		{
			Handle.Dispose();
			Handle = null;
		}
	}
}
