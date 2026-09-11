using System;
using System.IO;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class ApplicationCondition : FilterCondition, IDisposable
{
	private SafeWFPHandle _applicationIdHandle;

	public string ApplicationPath { get; private set; }

	public ApplicationCondition(string applicationPath)
	{
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_ALE_APP_ID;
		Condition.matchType = ConditionMatchType.Equal;
		ApplicationPath = applicationPath;
		if (string.IsNullOrEmpty(applicationPath))
		{
			Condition.conditionValue.type = FWP_DATA_TYPE.FWP_EMPTY;
			return;
		}
		if (!File.Exists(applicationPath))
		{
			throw new FileNotFoundException(applicationPath + " could not be found.");
		}
		Condition.conditionValue.type = FWP_DATA_TYPE.FWP_BYTE_BLOB_TYPE;
		_applicationIdHandle = SafeWFPHandle.CreateApplicationHandle(applicationPath);
		Condition.conditionValue.Union.byteBlob = _applicationIdHandle.DangerousGetHandle();
	}

	public void Dispose()
	{
		if (_applicationIdHandle.IsInvalid)
		{
			_applicationIdHandle?.Dispose();
		}
		GC.SuppressFinalize(this);
	}
}
