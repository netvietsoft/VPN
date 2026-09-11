namespace NextAiVPN.Services.Persistence;

internal interface IBrowserVersionHelper
{
	int GetEdgeVersion();

	int GetEdgeWebView2Version();

	int GetIeVersion();
}
