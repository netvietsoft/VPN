namespace NextAiVPN.Streaming.Entities;

internal class Server : IServer
{
	public string Name { get; }

	public string Status { get; }

	public int Utilization { get; }

	public Server(string name, string status, int utilization)
	{
		Name = name;
		Status = status;
		Utilization = utilization;
	}
}
