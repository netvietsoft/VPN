using System;
using VpnSDK.Private.WFP.Filtering;

namespace VpnSDK.Private.WFP;

internal interface IWFPEngine
{
	FilterCollection Filters { get; }

	bool InTransaction { get; }

	void StartWFPEngine(string engineName, bool runDynamically, bool enforceTransactionScope, ushort sublayerWeight);

	void Dispose();

	IDisposable StartTransaction(bool ignoreFilterProcessException = false);

	void CleanUp(string engineName);
}
