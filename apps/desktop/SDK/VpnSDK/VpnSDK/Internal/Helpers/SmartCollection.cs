using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace VpnSDK.Internal.Helpers;

internal class SmartCollection<T> : ObservableCollection<T>
{
	public SmartCollection()
	{
	}

	public SmartCollection(IEnumerable<T> items)
	{
		AddRange(items);
	}

	public void RemoveRange(IEnumerable<T> items)
	{
		CheckReentrancy();
		List<T> list = items?.ToList();
		bool flag = false;
		try
		{
			if (list != null && !list.Any())
			{
				return;
			}
			foreach (T item in list)
			{
				base.Items.Remove(item);
			}
		}
		finally
		{
			if (flag)
			{
				Reset();
			}
		}
	}

	public void AddRange(IEnumerable<T> items)
	{
		CheckReentrancy();
		List<T> list = items?.ToList();
		bool flag = false;
		try
		{
			if (list != null && !list.Any())
			{
				return;
			}
			foreach (T item in list)
			{
				base.Items.Add(item);
				flag = true;
			}
		}
		finally
		{
			if (flag)
			{
				Reset();
			}
		}
	}

	public void Repopulate(IEnumerable<T> items)
	{
		List<T> list = items?.ToList();
		if (list != null)
		{
			if (list.Count == 0)
			{
				Clear();
				Reset();
			}
			else
			{
				base.Items.Clear();
				AddRange(list);
			}
		}
	}

	public void Reset()
	{
		OnPropertyChanged(new PropertyChangedEventArgs("Count"));
		OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		SDKCore.SyncContext.Post(delegate
		{
			base.OnCollectionChanged(e);
		}, null);
	}
}
