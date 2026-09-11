namespace NextAiVPN.Services.Persistence;

public interface IBrowserLinksOpener
{
	void OpenBrowserLink(int link);

	void OpenBrowserLink(string link);
}
