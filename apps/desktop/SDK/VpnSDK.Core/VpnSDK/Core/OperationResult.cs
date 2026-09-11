namespace VpnSDK.Core;

internal class OperationResult
{
	public bool IsSuccess { get; }

	public int ErrorCode { get; }

	public static OperationResult Success => new OperationResult(isSuccess: true, 0);

	private OperationResult(bool isSuccess, int errorCode)
	{
		IsSuccess = isSuccess;
		ErrorCode = errorCode;
	}

	public static OperationResult Failure(int errorCode)
	{
		return new OperationResult(isSuccess: false, errorCode);
	}
}
