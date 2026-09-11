using System;
using VpnSDK.Private.WFP.Filtering;

namespace VpnSDK.Private.WFP;

internal interface IWFPManager
{
	FilterCollection Filters { get; }

	void StartGeneralEngine(string engineName, bool enforceTransactionScope = true);

	void StopGeneralEngine();

	IDisposable StartTransaction(bool ignoreFilterProcessException = false);

	void Dispose();

	void CleanUpExistingFilters(string engineName);
}
