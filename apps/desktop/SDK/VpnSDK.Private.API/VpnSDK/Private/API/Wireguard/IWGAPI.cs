using System.Threading;
using System.Threading.Tasks;
using RestEase;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Private.API.Wireguard;

[Header("Cache-Control", "no-cache")]
internal interface IWGAPI
{
	[Header("User-Agent")]
	string UserAgent { get; set; }

	[Post("generate")]
	[AllowAnyStatusCode]
	Task<Response<ConfigurationResponse>> GenerateAsync([Body(BodySerializationMethod.Serialized)] ConfigurationRequest request, CancellationToken token);
}
