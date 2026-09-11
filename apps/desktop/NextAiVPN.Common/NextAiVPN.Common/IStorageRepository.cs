using System.Threading.Tasks;

namespace NextAiVPN.Common;

public interface IStorageRepository
{
	Task SaveValue(string variableName, string value);

	Task<string> GetValue(string variableName);
}
