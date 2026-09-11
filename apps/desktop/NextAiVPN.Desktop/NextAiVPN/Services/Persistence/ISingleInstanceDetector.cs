namespace NextAiVPN.Services.Persistence;

internal interface ISingleInstanceDetector
{
	bool IsOneInstanceForEachUser();
}
