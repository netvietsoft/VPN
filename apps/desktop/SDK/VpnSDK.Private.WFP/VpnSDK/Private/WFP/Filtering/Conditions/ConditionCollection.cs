using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class ConditionCollection : Collection<FilterCondition>, IDisposable
{
	private readonly NetworkFilter _parentFilter;

	private readonly object _allocationBufferLock = new object();

	private bool _isDirty;

	private int _lastAllocatedBufferCount;

	internal IntPtr NativeConditionCollection { get; private set; } = IntPtr.Zero;

	internal int NativeConditionCollectionCount { get; private set; }

	internal ConditionCollection(NetworkFilter parent)
	{
		_parentFilter = parent;
	}

	private ConditionCollection()
	{
		throw new InvalidOperationException("What's wrong with you?");
	}

	private ConditionCollection(IList<FilterCondition> list)
		: base(list)
	{
	}

	~ConditionCollection()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected override void InsertItem(int index, FilterCondition newItem)
	{
		if (!_parentFilter.CanModify)
		{
			throw new InvalidOperationException("Attempted to modify condition collection outside of a transaction.");
		}
		if (newItem is IPAddressCondition ipCondition)
		{
			ValidateIpCondition(ipCondition);
		}
		base.InsertItem(index, newItem);
		_isDirty = true;
		_parentFilter.IsChanged = true;
	}

	protected override void RemoveItem(int index)
	{
		if (!_parentFilter.CanModify)
		{
			throw new InvalidOperationException("Attempted to modify condition collection outside of a transaction.");
		}
		base.RemoveItem(index);
		_isDirty = true;
		_parentFilter.IsChanged = true;
	}

	protected override void SetItem(int index, FilterCondition item)
	{
		if (!_parentFilter.CanModify)
		{
			throw new InvalidOperationException("Attempted to modify condition collection outside of a transaction.");
		}
		if (item is IPAddressCondition ipCondition)
		{
			ValidateIpCondition(ipCondition);
		}
		base.SetItem(index, item);
		_isDirty = true;
		_parentFilter.IsChanged = true;
	}

	public void AddRange(params FilterCondition[] items)
	{
		foreach (FilterCondition item in items)
		{
			Add(item);
		}
	}

	private void Dispose(bool disposing)
	{
		ReleaseUnmanagedResources();
		if (!disposing)
		{
			return;
		}
		foreach (IDisposable item in base.Items.Where((FilterCondition x) => x is IDisposable).Cast<IDisposable>())
		{
			item.Dispose();
		}
		Clear();
		_parentFilter._filter.filterCondition = IntPtr.Zero;
		_parentFilter._filter.numFilterConditions = 0u;
	}

	private void ReleaseUnmanagedResources()
	{
		lock (_allocationBufferLock)
		{
			if (NativeConditionCollection != IntPtr.Zero)
			{
				for (int i = 0; i < _lastAllocatedBufferCount; i++)
				{
					Marshal.DestroyStructure<FWPM_FILTER_CONDITION0>(NativeConditionCollection + Marshal.SizeOf<FWPM_FILTER_CONDITION0>() * i);
				}
				Marshal.FreeHGlobal(NativeConditionCollection);
				NativeConditionCollection = IntPtr.Zero;
				_lastAllocatedBufferCount = 0;
			}
		}
	}

	internal void UpdateUnmanagedConditionArray()
	{
		if (!_isDirty)
		{
			return;
		}
		ReleaseUnmanagedResources();
		lock (_allocationBufferLock)
		{
			_lastAllocatedBufferCount = base.Count;
			NativeConditionCollection = Marshal.AllocHGlobal(Marshal.SizeOf<FWPM_FILTER_CONDITION0>() * _lastAllocatedBufferCount);
			for (int i = 0; i < _lastAllocatedBufferCount; i++)
			{
				Marshal.StructureToPtr(base[i].Condition, NativeConditionCollection + Marshal.SizeOf<FWPM_FILTER_CONDITION0>() * i, fDeleteOld: false);
			}
			_parentFilter._filter.filterCondition = NativeConditionCollection;
			_parentFilter._filter.numFilterConditions = (uint)_lastAllocatedBufferCount;
		}
	}

	private void ValidateIpCondition(IPAddressCondition ipCondition)
	{
		if (_parentFilter.AddressFamily == IPAddressFamily.All)
		{
			switch (ipCondition.Address.AddressFamily)
			{
			case AddressFamily.InterNetwork:
				_parentFilter.AddressFamily = IPAddressFamily.IPv4;
				break;
			case AddressFamily.InterNetworkV6:
				_parentFilter.AddressFamily = IPAddressFamily.IPv6;
				break;
			}
		}
		else
		{
			if (_parentFilter.AddressFamily == IPAddressFamily.IPv4 && ipCondition.Address.AddressFamily == AddressFamily.InterNetworkV6)
			{
				throw new InvalidOperationException("Cannot add an IPv6 IP condition to an IPv4 filter.");
			}
			if (_parentFilter.AddressFamily == IPAddressFamily.IPv6 && ipCondition.Address.AddressFamily == AddressFamily.InterNetwork)
			{
				throw new InvalidOperationException("Cannot add an IPv4 IP condition to an IPv6 filter.");
			}
		}
	}
}
