using System;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.Utilities;

internal class NullStringConverter : JsonConverter<string>
{
	public override bool CanWrite => false;

	public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return null;
		}
		string text = reader.Value.ToString();
		if (string.IsNullOrWhiteSpace(text) || text.Equals("null", StringComparison.InvariantCultureIgnoreCase))
		{
			return null;
		}
		return text;
	}

	public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
