using System.Threading;
using System.Threading.Tasks;
using RestEase;
using VpnSDK.Private.API.Common.Enums;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Private.API;

[Header("Cache-Control", "no-cache")]
public interface IVpnApi
{
	[Header("X-API-Version", "3.4")]
	[Post("/login")]
	Task<User> Login([Body(BodySerializationMethod.Serialized)] LoginRequest login, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Post("/vpn")]
	Task<User> LoginUsingVpnEndpoint([Body(BodySerializationMethod.Serialized)] LoginRequest login, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Post("/login")]
	Task<User> RefreshTokenWithLogin([Body(BodySerializationMethod.Serialized)] TokenRequest tokenRequest, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Post("/logout")]
	Task<JsonResponseResult> Logout(CancellationToken cancellationToken);

	[Header("Authorization", "Bearer")]
	[Post("/refresh")]
	Task<User> RefreshToken([Body(BodySerializationMethod.Serialized)] TokenRequest tokenRequest, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Get("/servers?simple_type=true")]
	Task<string> GetServers(CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Get("/ipgeo")]
	Task<GeoIP> GetGeoLocation(CancellationToken cancellationToken);

	[Get("{url}")]
	Task<string> GetUrl([Path(UrlEncode = false)] string url, CancellationToken cancellationToken);

	[Get("/ping")]
	Task<string> GetPing(CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Get("/branding/{slugOrDomain}")]
	Task<BrandingInfo> GetBrandingData([Path] string slugOrDomain, [Header("X-Branding-Key")] string brandingKeyHeader, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Post("/wireguard")]
	[AllowAnyStatusCode]
	Task<ConfigurationResponse> GetWireguardConfig([Body(BodySerializationMethod.Serialized)] ConfigurationRequest request, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Get("/protocols/{protocolName}/config")]
	Task<ProtocolConfigResponse> GetProtocolConfig([Path] Protocol protocolName, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Get("/config")]
	Task<Response<AdditionalConfigResponse>> GetAdditionalConfig(CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Put("/account")]
	Task<AccountMetadataResponse> SaveAccountMetadata([Body(BodySerializationMethod.Serialized)] AccountMetadataRequest accountMetadataRequest, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Get("/account")]
	Task<AccountMetadataResponse> GetAccountMetadata(CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Post("/protocols/ikev2/setup")]
	Task<NetworkCredentialResponse> GetIKEv2VpnTokenCredentials([Body(BodySerializationMethod.Serialized)] SetUpRequest setUpRequest, CancellationToken cancellationToken);

	[Header("X-API-Version", "3.4")]
	[Post("/doublehop")]
	[AllowAnyStatusCode]
	Task<DoubleHopConfigurationResponse> GetDoubleHopConfig([Body(BodySerializationMethod.Serialized)] DoubleHopConfigurationRequest request, CancellationToken cancellationToken);
}
