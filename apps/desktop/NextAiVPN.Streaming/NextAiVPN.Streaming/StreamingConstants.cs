namespace NextAiVPN.Streaming;

internal static class StreamingConstants
{
	public static string ServerListEndpoint = "api/v1/vpn/config/serverlist";

	public static string VpnName = "NextAiVPN_Streaming";

	public static string IntermediateCertificateR12Name = "(STAGING) Riddling Rhubarb R12";

	public static string IntermediateCertificateR13Name = "(STAGING) Tenuous Tomato R13";

	public static string RootCertificatePPX1Name = "(STAGING) Pretend Pear X1";

	public static string RootCertificateYYRName = "(STAGING) Yonder Yam Root YR";

	public static string IntermediateCertificateR12FileName = "letsencrypt-stg-int-r12.pem";

	public static string IntermediateCertificateR13FileName = "letsencrypt-stg-int-r13.pem";

	public static string RootCertificatePPX1FileName = "letsencrypt-stg-root-x1.pem";

	public static string RootCertificateYYRFileName = "yonder-yam-root-yr.pem";

	public static string BaseEndPoint => "https://vpn.ncapi.io/";
}
