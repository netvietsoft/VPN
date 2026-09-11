using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VpnSDK.Private.API.Utilities;

internal class NetworkCredentialConverter : JsonConverter<NetworkCredential>
{
	public override void WriteJson(JsonWriter writer, NetworkCredential value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("Username");
		writer.WriteValue(value.UserName);
		writer.WritePropertyName("Password");
		writer.WriteValue(value.Password);
		writer.WriteEndObject();
	}

	public override NetworkCredential ReadJson(JsonReader reader, Type objectType, NetworkCredential existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType.HasFlag(JsonToken.StartObject))
		{
			JObject jObject = JObject.Load(reader);
			string userName = jObject.GetValue("username", StringComparison.InvariantCultureIgnoreCase).Value<string>();
			string password = jObject.GetValue("password", StringComparison.InvariantCultureIgnoreCase).Value<string>();
			return new NetworkCredential(userName, password);
		}
		if (reader.TokenType.HasFlag(JsonToken.StartArray))
		{
			JArray jArray = JArray.Load(reader);
			if (jArray.Count < 2)
			{
				throw new JsonReaderException("Array not big enough to convert.");
			}
			return new NetworkCredential((string?)jArray[0], (string?)jArray[1]);
		}
		throw new JsonSerializationException();
	}
}
