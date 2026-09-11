using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VpnSDK.Private.API.Utilities;

internal class PrivateSetterCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
{
	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
		if (jsonProperty.Writable)
		{
			return jsonProperty;
		}
		jsonProperty.Writable = member.IsPropertyWithSetter();
		return jsonProperty;
	}
}
