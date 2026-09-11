using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP;

public class WFPException : Exception
{
	public WFPError ErrorType { get; private set; }

	internal WFPException()
	{
	}

	internal WFPException(WFPError error)
		: base(GetDescription(error))
	{
		ErrorType = error;
		base.HResult = (int)error;
	}

	internal WFPException(string message)
		: base(message)
	{
	}

	internal WFPException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected internal WFPException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string GetDescription(Enum value)
	{
		DescriptionAttribute[] array = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
		if (array.Length == 0)
		{
			return value.ToString();
		}
		return array[0].Description;
	}
}
