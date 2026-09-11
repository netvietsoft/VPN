using System;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.NewtonsoftJson;

namespace NextAiVPN;

internal static class RestClientFactory
{
	public static RestClient Create(RestClientOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return new RestClient(options, null, delegate(SerializerConfig serializerConfig)
		{
			serializerConfig.UseNewtonsoftJson();
		});
	}
}
