using System;
using System.Text;

namespace VpnSDK.Private.API.Common.Extensions;

public static class ExceptionExtensions
{
	public static T GetInnerException<T>(this Exception ex) where T : Exception
	{
		for (Exception ex2 = ex; ex2 != null; ex2 = ex2.InnerException)
		{
			if (ex2 is T result)
			{
				return result;
			}
		}
		return null;
	}

	public static string GetFormattedExceptionMessages(this Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Exception ex2 = ex;
		while (ex2 != null)
		{
			stringBuilder.Append($"{ex2.GetType()}: {ex2.Message}");
			ex2 = ex2.InnerException;
			if (ex2 != null)
			{
				stringBuilder.Append(" ---> ");
			}
		}
		return stringBuilder.ToString();
	}
}
