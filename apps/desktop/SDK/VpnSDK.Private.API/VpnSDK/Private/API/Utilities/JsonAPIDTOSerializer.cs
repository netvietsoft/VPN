using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Private.API.Utilities;

public static class JsonAPIDTOSerializer
{
	private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
	{
		ContractResolver = new PrivateSetterContractResolver(),
		Converters = new JsonConverter[4]
		{
			new IPAddressConverter(),
			new UnixDateTimeConverter(),
			new NullStringConverter(),
			new NetworkCredentialConverter()
		},
		PreserveReferencesHandling = PreserveReferencesHandling.Objects,
		TypeNameHandling = TypeNameHandling.All,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore
	};

	public static void Serialize(object obj, string filepath)
	{
		if (!(obj is Node) && !(obj is JsonResponseResult))
		{
			throw new SerializationException("Object provided is not an API DTO.");
		}
		using TextWriter textWriter = File.CreateText(filepath);
		JsonSerializer.Create(_settings).Serialize(textWriter, obj);
	}

	public static T Deserialize<T>(string filepath)
	{
		using StreamReader reader = new StreamReader(filepath);
		using JsonReader reader2 = new JsonTextReader(reader);
		return JsonSerializer.Create(_settings).Deserialize<T>(reader2);
	}
}
