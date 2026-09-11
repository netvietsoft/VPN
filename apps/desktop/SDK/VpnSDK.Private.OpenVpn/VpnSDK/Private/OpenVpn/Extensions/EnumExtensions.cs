using System;
using System.ComponentModel;

namespace VpnSDK.Private.OpenVpn.Extensions;

public static class EnumExtensions
{
	public static T GetAttribute<T>(this Enum enumVal) where T : Attribute
	{
		object[] customAttributes = enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttributes(typeof(T), inherit: false);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return (T)customAttributes[0];
	}

	public static string GetDescription(this Enum enumVal)
	{
		DescriptionAttribute attribute = enumVal.GetAttribute<DescriptionAttribute>();
		if (attribute == null)
		{
			return enumVal.ToString();
		}
		return attribute.Description;
	}
}
