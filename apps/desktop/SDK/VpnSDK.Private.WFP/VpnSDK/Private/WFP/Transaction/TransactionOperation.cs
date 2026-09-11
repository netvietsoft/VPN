using System.Collections.Generic;
using VpnSDK.Private.WFP.Filtering;

namespace VpnSDK.Private.WFP.Transaction;

internal class TransactionOperation
{
	public TransactionOperationType Operation { get; private set; }

	public NetworkFilter Filter { get; private set; }

	public List<ulong> NewID { get; } = new List<ulong>();

	public bool HasExecuted { get; set; }

	public TransactionOperation(TransactionOperationType operationType, NetworkFilter filter)
	{
		Operation = operationType;
		Filter = filter;
	}

	public void Flush()
	{
		if (HasExecuted)
		{
			if (Operation == TransactionOperationType.Remove || Operation == TransactionOperationType.Update)
			{
				Filter.FilterIds.Clear();
			}
			if (Operation == TransactionOperationType.Add || Operation == TransactionOperationType.Update)
			{
				Filter.FilterIds.AddRange(NewID);
			}
			Filter.IsChanged = false;
		}
	}
}
