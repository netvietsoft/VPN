using System.Reflection;

namespace VpnSDK.Private.API.Utilities;

internal static class MemberInfoExtensions
{
	internal static bool IsPropertyWithSetter(this MemberInfo member)
	{
		return (member as PropertyInfo)?.GetSetMethod(nonPublic: true) != null;
	}
}
