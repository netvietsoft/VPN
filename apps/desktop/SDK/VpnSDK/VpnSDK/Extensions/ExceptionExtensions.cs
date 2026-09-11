using System;
using System.Collections.Generic;

namespace VpnSDK.Extensions;

public static class ExceptionExtensions
{
	public static IEnumerable<Exception> GetAllExceptions(this Exception ex)
	{
		Exception currentEx = ex;
		yield return currentEx;
		while (currentEx.InnerException != null)
		{
			currentEx = currentEx.InnerException;
			yield return currentEx;
		}
	}

	public static IEnumerable<string> GetAllExceptionMessages(this Exception ex)
	{
		Exception currentEx = ex;
		yield return currentEx.Message;
		while (currentEx.InnerException != null)
		{
			currentEx = currentEx.InnerException;
			yield return currentEx.Message;
		}
	}
}
