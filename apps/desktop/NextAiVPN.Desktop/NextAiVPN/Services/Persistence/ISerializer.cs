namespace NextAiVPN.Services.Persistence;

internal interface ISerializer<T>
{
	void Serialize(T data, string filePath);

	T Deserialize(string filePath);
}
