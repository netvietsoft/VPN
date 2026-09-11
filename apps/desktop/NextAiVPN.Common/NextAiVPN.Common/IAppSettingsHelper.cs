namespace NextAiVPN.Common;

public interface IAppSettingsHelper
{
	void SetValue(string variableName, string value);

	string GetValue(string variableName);
}
