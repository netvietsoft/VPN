using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VpnSDK.Private.WFP.Transaction;

namespace VpnSDK.Private.WFP.Filtering;

public class FilterCollection : Collection<NetworkFilter>, IDisposable
{
	private bool _isDisposed;

	private readonly WFPEngineBase _engine;

	private volatile bool _inUndoOperation;

	internal List<TransactionOperation> Transactions { get; private set; } = new List<TransactionOperation>();

	public NetworkFilter this[string name] => this.First((NetworkFilter x) => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

	private FilterCollection()
	{
		throw new InvalidOperationException("How did you do this?");
	}

	internal FilterCollection(WFPEngineBase engine)
	{
		_engine = engine;
	}

	public void AddRange(params NetworkFilter[] items)
	{
		foreach (NetworkFilter item in items)
		{
			Add(item);
		}
	}

	protected override void InsertItem(int index, NetworkFilter newItem)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("FilterCollection");
		}
		if (_inUndoOperation)
		{
			base.InsertItem(index, newItem);
			return;
		}
		if (!_engine.InTransaction)
		{
			throw new InvalidOperationException("Attempted to add filter outside of transaction scope.");
		}
		if (!Contains(newItem))
		{
			Transactions.Add(new TransactionOperation(TransactionOperationType.Add, newItem));
			base.InsertItem(index, newItem);
		}
	}

	protected override void RemoveItem(int index)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("FilterCollection");
		}
		if (_inUndoOperation)
		{
			base.RemoveItem(index);
			return;
		}
		if (!_engine.InTransaction)
		{
			throw new InvalidOperationException("Attempted to remove filter outside of transaction scope.");
		}
		Transactions.Add(new TransactionOperation(TransactionOperationType.Remove, base[index]));
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, NetworkFilter item)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("FilterCollection");
		}
		if (_inUndoOperation)
		{
			base.SetItem(index, item);
			return;
		}
		if (!_engine.InTransaction)
		{
			throw new InvalidOperationException("Attempted to replace filter outside of transaction scope.");
		}
		Transactions.Add(new TransactionOperation(TransactionOperationType.Remove, base[index]));
		Transactions.Add(new TransactionOperation(TransactionOperationType.Add, item));
		base.SetItem(index, item);
	}

	protected override void ClearItems()
	{
		if (_inUndoOperation)
		{
			base.ClearItems();
			return;
		}
		if (!_engine.InTransaction)
		{
			throw new InvalidOperationException("Attempted to clear filters outside of transaction scope.");
		}
		Transactions.AddRange(this.Select((NetworkFilter x) => new TransactionOperation(TransactionOperationType.Remove, x)));
		base.ClearItems();
	}

	public bool TryGet(string name, out NetworkFilter filter)
	{
		filter = this.FirstOrDefault((NetworkFilter x) => x.Name == name);
		return filter != null;
	}

	internal void Undo()
	{
		_inUndoOperation = true;
		foreach (TransactionOperation transaction in Transactions)
		{
			if (transaction.Operation == TransactionOperationType.Add)
			{
				Add(transaction.Filter);
			}
			else if (transaction.Operation == TransactionOperationType.Remove)
			{
				Remove(transaction.Filter);
			}
		}
		Transactions.Clear();
		_inUndoOperation = false;
	}

	private void Dispose(bool disposing)
	{
		if (!disposing || _isDisposed)
		{
			return;
		}
		using (IEnumerator<NetworkFilter> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				NetworkFilter current = enumerator.Current;
				current.FilterIds.Clear();
				current._associatedEngine = null;
			}
		}
		_inUndoOperation = true;
		using (IEnumerator<NetworkFilter> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current._associatedEngine = null;
			}
		}
		Clear();
		Transactions.Clear();
		_inUndoOperation = false;
		_isDisposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~FilterCollection()
	{
		Dispose(disposing: false);
	}
}
