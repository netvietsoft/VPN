using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextAiVPN.Services.Persistence;

public interface IVpnModeValidator
{
	Task<List<VpnType>> Validate(bool showStreamingMessageBox);
}
