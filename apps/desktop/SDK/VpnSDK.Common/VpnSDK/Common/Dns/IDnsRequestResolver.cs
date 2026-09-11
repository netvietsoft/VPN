using System.Threading.Tasks;

namespace VpnSDK.Common.Dns;

public interface IDnsRequestResolver
{
	Task<DnsResolutionResult> ResolveAsync(string domain);
}
