using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VpnSDK.Private.WFP.Filtering.Conditions;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering;

public class NetworkFilter : INotifyPropertyChanged, IDisposable
{
	private static bool OS_SUPPORTS_INDEXING = Environment.OSVersion.Version >= new Version(6, 2, 9200, 0);

	internal WFPEngineBase _associatedEngine;

	internal FWPM_FILTER0 _filter;

	private volatile bool _disablePropertyChange;

	[CompilerGenerated]
	private IPAddressFamily _003CAddressFamily_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CEnabled_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CIsChanged_003Ek__BackingField;

	public IPAddressFamily AddressFamily
	{
		[CompilerGenerated]
		get
		{
			return _003CAddressFamily_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			if (_003CAddressFamily_003Ek__BackingField != value)
			{
				object before = AddressFamily;
				_003CAddressFamily_003Ek__BackingField = value;
				object after = AddressFamily;
				OnPropertyChanged("AddressFamily", before, after);
			}
		}
	}

	public bool Allow
	{
		get
		{
			return _filter.action;
		}
		set
		{
			if (Allow != value)
			{
				object before = Allow;
				_filter.action = value;
				object after = Allow;
				OnPropertyChanged("Allow", before, after);
			}
		}
	}

	public bool CanOverride
	{
		get
		{
			return _filter.flags.HasFlag(WFPFilterFlag.ClearActionRight);
		}
		set
		{
			if (CanOverride != value)
			{
				object before = CanOverride;
				if (value)
				{
					_filter.flags |= WFPFilterFlag.ClearActionRight;
				}
				else
				{
					_filter.flags &= ~WFPFilterFlag.ClearActionRight;
				}
				object after = CanOverride;
				OnPropertyChanged("CanOverride", before, after);
			}
		}
	}

	public ConditionCollection Conditions { get; }

	public bool Enabled
	{
		[CompilerGenerated]
		get
		{
			return _003CEnabled_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			if (_003CEnabled_003Ek__BackingField != value)
			{
				object before = Enabled;
				_003CEnabled_003Ek__BackingField = value;
				object after = Enabled;
				OnPropertyChanged("Enabled", before, after);
			}
		}
	}

	public bool IsChanged
	{
		[CompilerGenerated]
		get
		{
			return _003CIsChanged_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			if (_003CIsChanged_003Ek__BackingField != value)
			{
				object before = IsChanged;
				_003CIsChanged_003Ek__BackingField = value;
				object after = IsChanged;
				OnPropertyChanged("IsChanged", before, after);
			}
		}
	}

	public string Name
	{
		get
		{
			return _filter.displayData.Name;
		}
		private set
		{
			if (!string.Equals(Name, value, StringComparison.Ordinal))
			{
				object name = Name;
				_filter.displayData.Name = value;
				object name2 = Name;
				OnPropertyChanged("Name", name, name2);
			}
		}
	}

	public FilterWeight Weight
	{
		get
		{
			return (FilterWeight)_filter.weight.Union.uint8;
		}
		set
		{
			if (Weight != value)
			{
				object before = Weight;
				_filter.weight.type = ((value != FilterWeight.Auto) ? FWP_DATA_TYPE.FWP_UINT8 : FWP_DATA_TYPE.FWP_EMPTY);
				_filter.weight.Union.uint8 = (byte)value;
				object after = Weight;
				OnPropertyChanged("Weight", before, after);
			}
		}
	}

	internal bool CanModify
	{
		get
		{
			if (_associatedEngine != null)
			{
				WFPEngineBase associatedEngine = _associatedEngine;
				if ((associatedEngine == null || !associatedEngine.TransactionScopeEnforcement || !_associatedEngine.InTransaction) && InEngine)
				{
					WFPEngineBase associatedEngine2 = _associatedEngine;
					if (associatedEngine2 == null)
					{
						return false;
					}
					return !associatedEngine2.TransactionScopeEnforcement;
				}
			}
			return true;
		}
	}

	internal List<ulong> FilterIds { get; }

	internal bool InEngine => FilterIds.Count > 0;

	public event PropertyChangedEventHandler PropertyChanged;

	public NetworkFilter(string name, params FilterCondition[] conditions)
	{
		AddressFamily = IPAddressFamily.All;
		Enabled = true;
		FilterIds = new List<ulong>();
		base._002Ector();
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		Conditions = new ConditionCollection(this);
		_filter.action.type = 4098u;
		Name = name;
		if (OS_SUPPORTS_INDEXING)
		{
			_filter.flags |= WFPFilterFlag.Indexed;
		}
		if (conditions != null && conditions.Length != 0)
		{
			Conditions.AddRange(conditions);
		}
	}

	~NetworkFilter()
	{
	}

	public void Dispose()
	{
		Conditions.Dispose();
	}

	internal void OnPropertyChanged(string propertyName, object before, object after)
	{
		if (_disablePropertyChange)
		{
			_disablePropertyChange = false;
			return;
		}
		if (!CanModify)
		{
			_disablePropertyChange = true;
			GetType().GetProperty(propertyName)?.SetValue(this, before);
			throw new InvalidOperationException("Modification of network filter happened outside of a transaction.");
		}
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
