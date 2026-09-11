namespace NextAiVPN.Streaming.Entities;

public interface IServer
{
	string Name { get; }

	string Status { get; }

	int Utilization { get; }
}
