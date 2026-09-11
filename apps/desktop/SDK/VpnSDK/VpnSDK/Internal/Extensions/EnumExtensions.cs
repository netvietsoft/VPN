using System;
using System.ComponentModel;

namespace VpnSDK.Internal.Extensions;

internal static class EnumExtensions
{
	public static string Description<T>(this T source) where T : struct, IConvertible
	{
		DescriptionAttribute[] array = (DescriptionAttribute[])source.GetType().GetField(source.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
		if (array != null && array.Length != 0)
		{
			return array[0].Description;
		}
		return source.ToString();
	}
}
