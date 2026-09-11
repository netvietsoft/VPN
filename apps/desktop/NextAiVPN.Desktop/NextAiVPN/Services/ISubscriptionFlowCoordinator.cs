using System;
using System.Threading.Tasks;
using NextAiVPN.Enums;

namespace NextAiVPN.Services;

public interface ISubscriptionFlowCoordinator
{
	event Action TrialFirstWindowRequested;

	event Action<TrialChipsUpdate> TrialChipsUpdateRequested;

	Task<bool> IsSubscriptionActiveAsync();

	Task ShowTrialFirstIfNeededAsync();

	string GetNoSubscriptionControl();
}
